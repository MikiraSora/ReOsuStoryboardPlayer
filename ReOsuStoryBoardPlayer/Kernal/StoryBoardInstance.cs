using OpenTK.Graphics.OpenGL;
using ReOsuStoryBoardPlayer.Base;
using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReOsuStoryBoardPlayer
{
    public class StoryBoardInstance
    {
        private static readonly uint DrawCallInstanceCountMax = 50;

        public LinkedList<StoryBoardObject> StoryboardObjectList { get; set; }

        private LinkedListNode<StoryBoardObject> CurrentScanNode;

        private List<StoryBoardObject> DrawSplitList = new List<StoryBoardObject>();

        internal Dictionary<Layout, List<StoryBoardObject>> _UpdatingStoryBoard = new Dictionary<Layout, List<StoryBoardObject>>();

        internal string osb_file_path = string.Empty, osu_file_path = string.Empty, audio_file_path = string.Empty, folder_path = string.Empty;

        public MusicPlayer player;

        //internal float update_current_time;

        private Stopwatch runTimer = new Stopwatch();

        public long UpdateCastTime { get; private set; }

        public long RenderCastTime { get; private set; }

        public bool IsWideScreen { get; set; } = false;

        private DebugToolInstance debug_instance;
        public DebugToolInstance DebugToolInstance { get => debug_instance; }

        public StoryBoardInstance(string folder_path)
        {
            if (!folder_path.EndsWith(@"\"))
            {
                folder_path += @"\";
            }

            this.folder_path = folder_path;

            StoryboardObjectList = new LinkedList<StoryBoardObject>();

            CurrentScanNode = StoryboardObjectList.First;

            //int audioLeadIn = 0;

            #region Get files path

            foreach (var path in Directory.EnumerateFiles(folder_path))
            {
                if (path.EndsWith(@".osb"))
                {
                    osb_file_path = path;
                    Log.User($"osb file path={path}");
                }
                else if (path.EndsWith(@".osu"))
                {
                    osu_file_path = path;
                    Log.User($"osu file path={path}");

                    string content = File.ReadAllText(path);
                    var match = Regex.Match(content, @"AudioFilename\s*:\s*(.+)");

                    //audioLeadIn = int.Parse(Regex.Match(content, @"AudioLeadIn:\s*(.+)").Groups[1].Value.Replace("\r", string.Empty));

                    if (true)
                    {
                        audio_file_path = folder_path + match.Groups[1].Value.Replace("\r", string.Empty);
                        Log.User($"audio file path={audio_file_path}");
                    }

                    //WidescreenStoryboard
                    match = Regex.Match(content, @"WidescreenStoryboard\s*:\s*(.+)");
                    if (match.Success)
                    {
                        IsWideScreen = match.Groups[1].Value.ToString().Trim() == "1";
                    }
                }
            }

            #region Check

            if (string.IsNullOrWhiteSpace(osu_file_path) || string.IsNullOrWhiteSpace(audio_file_path) || (!File.Exists(osu_file_path) || (!File.Exists(audio_file_path))))
            {
                Console.WriteLine("无法获取到osu文件或者音频文件路径");
                Environment.Exit(0);
            }

            #endregion Check

            #endregion Get files path

            #region Load and Parse osb/osu file

            using (StopwatchRun.Count("Load and Parse osb/osu file"))
            {
                List<StoryBoardObject> temp_objs_list = new List<StoryBoardObject>(), parse_osb_storyboard_objs = new List<StoryBoardObject>();

                //get objs from osu file
                List<StoryBoardObject> parse_osu_storyboard_objs = StoryboardParserHelper.GetStoryBoardObjects(osu_file_path);

                if ((!string.IsNullOrWhiteSpace(osb_file_path)) && File.Exists(osb_file_path))
                {
                    parse_osb_storyboard_objs = StoryboardParserHelper.GetStoryBoardObjects(osb_file_path);
                    AdjustZ(parse_osb_storyboard_objs, 0);
                }

                AdjustZ(parse_osu_storyboard_objs, parse_osb_storyboard_objs?.Count() ?? 0);

                temp_objs_list = CombineStoryBoardObjects(parse_osb_storyboard_objs, parse_osu_storyboard_objs);

                //delete Background object if there is a normal storyboard object which is same image file.
                var background_obj = temp_objs_list.Where(c => c is StoryboardBackgroundObject).FirstOrDefault();
                if (temp_objs_list.Any(c => c.ImageFilePath == background_obj?.ImageFilePath && (!(c is StoryboardBackgroundObject))))
                {
                    Log.User($"Found another same background image object and delete background object.");
                    temp_objs_list.Remove(background_obj);
                }
                else
                {
                    if (background_obj != null)
                        background_obj.Z = -1;
                }

                foreach (var obj in temp_objs_list)
                    StoryboardObjectList.AddLast(obj);

                StoryboardObjectList.AsParallel().ForAll(c => c.SortCommands());
            }

            #endregion Load and Parse osb/osu file

            #region Create LayoutListMap

            foreach (Layout item in Enum.GetValues(typeof(Layout)))
            {
                _UpdatingStoryBoard.Add(item, new List<StoryBoardObject>());
            }

            #endregion Create LayoutListMap

            player = new MusicPlayer(audio_file_path);

            player.OnJumpCurrentPlayingTime += Player_OnJumpCurrentPlayingTime;

            debug_instance = new DebugToolInstance(this);

            void AdjustZ(List<StoryBoardObject> list, int base_z)
            {
                list.Sort((a, b) => (int)(a.FileLine - b.FileLine));
                for (int i = 0; i < list.Count; i++)
                    list[i].Z = base_z + i;
            }
        }

        private List<StoryBoardObject> CombineStoryBoardObjects(List<StoryBoardObject> osb_list, List<StoryBoardObject> osu_list)
        {
            #region Safe Check

            if (osb_list == null)
            {
                osb_list = new List<StoryBoardObject>();
            }

            if (osu_list == null)
            {
                osu_list = new List<StoryBoardObject>();
            }

            #endregion Safe Check

            List<StoryBoardObject> result = new List<StoryBoardObject>(osb_list);
            result.AddRange(osu_list);

            result.Sort((a, b) =>
            {
                return a.FrameStartTime - b.FrameStartTime;
            });

            return result;
        }

        private void Player_OnJumpCurrentPlayingTime(uint new_time)
        {
            //fast seek? tan 90°
            Flush();

            //update_current_time = new_time;
        }

        internal void BuildCacheDrawSpriteBatch()
        {
            var obj_list = StoryboardObjectList.ToList();

            List<string> pic_list = new List<string>();
            pic_list.AddRange(Directory.EnumerateFiles(folder_path, "*.png", SearchOption.AllDirectories));
            pic_list.AddRange(Directory.EnumerateFiles(folder_path, "*.jpg", SearchOption.AllDirectories));

            Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap = new Dictionary<string, SpriteInstanceGroup>();

            pic_list.ForEach((path) =>
            {
                Texture tex = new Texture(path);
                string absolute_path = path.Replace(folder_path, string.Empty).Trim();

                CacheDrawSpriteInstanceMap[absolute_path.ToLower()] = new SpriteInstanceGroup(DrawCallInstanceCountMax, absolute_path, tex);

                Log.Debug($"Created storyboard sprite instance from image file :{path}");
            });

            obj_list.ForEach(obj =>
            {
                switch (obj)
                {
                    case StoryboardBackgroundObject background:
                        if (!CacheDrawSpriteInstanceMap.TryGetValue(obj.ImageFilePath.ToLower(), out obj.RenderGroup))
                            Log.Warn($"not found image:{obj.ImageFilePath}");

                        if (background.RenderGroup != null)
                        {
                            var scale = StoryboardWindow.SB_HEIGHT / background.RenderGroup.Texture.Height;
                            background.AddCommand(new ScaleCommand()
                            {
                                Easing = EasingConverter.CacheEasingInterpolatorMap[Easing.Linear],
                                StartTime = -2857,
                                EndTime = -2857,
                                StartValue = scale,
                                EndValue = scale
                            });
                        }
                        break;

                    case StoryboardAnimation animation:
                        List<SpriteInstanceGroup> list = new List<SpriteInstanceGroup>();

                        for (int index = 0; index < animation.FrameCount; index++)
                        {
                            SpriteInstanceGroup group;
                            string path = animation.FrameBaseImagePath + index + animation.FrameFileExtension;
                            if (!CacheDrawSpriteInstanceMap.TryGetValue(path, out group))
                            {
                                Log.Warn($"not found image:{path}");
                                continue;
                            }
                            list.Add(group);
                        }

                        animation.backup_group = list.ToArray();
                        break;

                    default:
                        if (!CacheDrawSpriteInstanceMap.TryGetValue(obj.ImageFilePath.ToLower(), out obj.RenderGroup))
                            Log.Warn($"not found image:{obj.ImageFilePath}");
                        break;
                }
            });
        }

        public void Start()
        {
            player.Play();
            CurrentScanNode = StoryboardObjectList.First;
        }

        public void Flush()
        {
            foreach (var pair in _UpdatingStoryBoard)
            {
                pair.Value.Clear();
            }

            CurrentScanNode = StoryboardObjectList.First;

            StoryboardObjectList.AsParallel().ForAll((obj) => obj.markDone = false);
        }

        private bool Scan(float current_time)
        {
            LinkedListNode<StoryBoardObject> LastAddNode = null;

            while (CurrentScanNode != null && CurrentScanNode.Value.FrameStartTime <= current_time/* && current_time <= CurrentScanNode.Value.FrameEndTime*/ )
            {
                var obj = CurrentScanNode.Value;
                if (current_time > obj.FrameEndTime)
                {
                    CurrentScanNode = CurrentScanNode.Next;
                    continue;
                }

                obj.markDone = false;
                _UpdatingStoryBoard[obj.layout].Add(obj);

                LastAddNode = CurrentScanNode;

                CurrentScanNode = CurrentScanNode.Next;
            }

            if (LastAddNode != null)
            {
                CurrentScanNode = LastAddNode.Next;
            }

            return /*isAdd*/LastAddNode != null;
        }

        public void Update(float delay_time)
        {
            var t = runTimer.ElapsedMilliseconds;

            player.Tick();

            float current_time = player.CurrentFixedTime;

            bool hasAdded = Scan(current_time);

            foreach (var objs in _UpdatingStoryBoard.Values)
            {
                if (hasAdded)
                {
                    objs.Sort((a, b) =>
                    {
                        return a.Z - b.Z;
                    });
                }

                foreach (var obj in objs)
                {
                    if (current_time < obj.FrameStartTime || current_time > obj.FrameEndTime)
                        obj.markDone = true;
                    else
                        obj.Update(current_time);
                }
            }

            //remove unused objects
            foreach (var objs in _UpdatingStoryBoard.Values)
            {
                objs.RemoveAll((obj) =>
                {
                    if (!obj.markDone)
                        return false;

                    return true;
                });
            }

#if DEBUG

            debug_instance.Update();

#endif

            UpdateCastTime = runTimer.ElapsedMilliseconds - t;
        }

        #region Storyboard Rendering

        public void PostDrawStoryBoard()
        {
            var r = runTimer.ElapsedMilliseconds;

            foreach (var layout_list in _UpdatingStoryBoard)
            {
                if (layout_list.Value.Count == 0)
                {
                    continue;
                }

                DrawStoryBoardObjects(layout_list.Value);
            }

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            RenderCastTime = runTimer.ElapsedMilliseconds - r;
        }

        private void DrawStoryBoardObjects(List<StoryBoardObject> draw_list)
        {
            SpriteInstanceGroup group = draw_list.First().RenderGroup;

            bool additive_trigger;
            ChangeAdditiveStatus(draw_list.First().IsAdditive);

            for (int i = 0; i < draw_list.Count; i++)
            {
                var obj = draw_list[i];

                if (obj.Color.w <= 0)
                    continue;//skip

                if (group != obj.RenderGroup || additive_trigger != obj.IsAdditive)
                {
                    group?.FlushDraw();

                    //应该是现在设置Blend否则Group自动渲染来不及钦定
                    ChangeAdditiveStatus(obj.IsAdditive);

                    group = obj.RenderGroup;
                }

                group?.PostRenderCommand(obj.Postion, obj.Z, obj.Rotate, obj.Scale, obj.Anchor, obj.Color, obj.IsVerticalFlip, obj.IsHorizonFlip);
            }

            //ChangeAdditiveStatus(false);

            if (group?.CurrentPostCount != 0)
                group?.FlushDraw();

            void ChangeAdditiveStatus(bool is_additive_blend)
            {
                additive_trigger = is_additive_blend;
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, additive_trigger ? BlendingFactorDest.One : BlendingFactorDest.OneMinusSrcAlpha);
            }
        }

        #endregion Storyboard Rendering

        ~StoryBoardInstance()
        {
            player.Term();
        }
    }
}