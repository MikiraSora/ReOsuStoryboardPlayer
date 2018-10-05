using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using System.Diagnostics;
using System.Threading;
using ReOsuStoryBoardPlayer.DebugTool.ObjectInfoVisualizer;
using ReOsuStoryBoardPlayer.Parser.Stream;
using ReOsuStoryBoardPlayer.Parser.Collection;
using ReOsuStoryBoardPlayer.Parser.Reader;
using static System.Collections.Specialized.BitVector32;
using ReOsuStoryBoardPlayer.Parser;

namespace ReOsuStoryBoardPlayer
{
    public class StoryBoardInstance
    {
        static readonly uint DrawCallInstanceCountMax = 50;

        internal LinkedList<StoryBoardObject> StoryboardObjectList;

        LinkedListNode<StoryBoardObject> CurrentScanNode;

        List<StoryBoardObject> DrawSplitList = new List<StoryBoardObject>();

        internal Dictionary<Layout, List<StoryBoardObject>> _UpdatingStoryBoard = new Dictionary<Layout, List<StoryBoardObject>>();
        
        internal string osb_file_path = string.Empty, osu_file_path = string.Empty, audio_file_path = string.Empty,folder_path=string.Empty;

        public MusicPlayer player;

        //internal float update_current_time;

        Stopwatch runTimer = new Stopwatch();

        public long UpdateCastTime { get; private set; }

        public long RenderCastTime { get; private set; }

        public bool IsWideScreen { get; set; } = false;

#if DEBUG
        DebugToolInstance debug_instance;
        public DebugToolInstance DebugToolInstance { get => debug_instance; }
#endif

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
                        audio_file_path =folder_path + match.Groups[1].Value.Replace("\r",string.Empty);
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

            if (string.IsNullOrWhiteSpace(osu_file_path)||string.IsNullOrWhiteSpace(audio_file_path)||(!File.Exists(osu_file_path)||(!File.Exists(audio_file_path))))
            {
                Console.WriteLine("无法获取到osu文件或者音频文件路径");
                Environment.Exit(0);
            }

            #endregion

            #endregion

            #region Load and Parse osb/osu file

            List<StoryBoardObject> temp_objs_list = new List<StoryBoardObject>(), parse_osb_storyboard_objs=new List<StoryBoardObject>();
            /*
            Task<List<StoryBoardObject>>[] tasks = new[] {
                Task.Run(()=>StoryboardParserHelper.GetStoryBoardObjects(osu_file_path)),
                Task.Run(()=>StoryboardParserHelper.GetStoryBoardObjects(osb_file_path))
            };
            
            Task.WaitAll(tasks);
            */
            //get objs from osu file
            List<StoryBoardObject> parse_osu_storyboard_objs = StoryboardParserHelper.GetStoryBoardObjects(osu_file_path);
            
            if ((!string.IsNullOrWhiteSpace(osb_file_path))&&File.Exists(osb_file_path))
            {
                parse_osb_storyboard_objs = StoryboardParserHelper.GetStoryBoardObjects(osb_file_path);
                AdjustZ(parse_osb_storyboard_objs, 0);
            }

            AdjustZ(parse_osu_storyboard_objs, parse_osb_storyboard_objs?.Count()??0);


            temp_objs_list = CombineStoryBoardObjects(parse_osb_storyboard_objs, parse_osu_storyboard_objs);

            foreach (var obj in temp_objs_list)
            {
                StoryboardObjectList.AddLast(obj);
            }

            #endregion

            #region Create LayoutListMap
            
            foreach (Layout item in Enum.GetValues(typeof(Layout)))
            {
                _UpdatingStoryBoard.Add(item, new List<StoryBoardObject>());
            }

            #endregion

            player = new MusicPlayer(audio_file_path);

            player.OnJumpCurrentPlayingTime += Player_OnJumpCurrentPlayingTime;

#if DEBUG
            debug_instance = new DebugToolInstance(this);
#endif

            void AdjustZ(List<StoryBoardObject> list,int base_z)
            {
                list.Sort((a,b)=> (int)(a.FileLine-b.FileLine));
                for (int i = 0; i < list.Count; i++)
                    list[i].Z = base_z+i;
            }
        }

        private List<StoryBoardObject> CombineStoryBoardObjects(List<StoryBoardObject> osb_list,List<StoryBoardObject> osu_list)
        {
            #region Safe Check

            if (osb_list==null)
            {
                osb_list = new List<StoryBoardObject>();
            }

            if (osu_list==null)
            {
                osu_list = new List<StoryBoardObject>();
            }

            #endregion

            List<StoryBoardObject> result = new List<StoryBoardObject>(osb_list);
            result.AddRange(osu_list);

            result.Sort((a,b)=> {
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

            List<String> pic_list = new List<string>();
            pic_list.AddRange(Directory.EnumerateFiles(folder_path, "*.png", SearchOption.AllDirectories));
            pic_list.AddRange(Directory.EnumerateFiles(folder_path, "*.jpg", SearchOption.AllDirectories));

            Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap=new Dictionary<string, SpriteInstanceGroup>();

            pic_list.ForEach((path)=>
            {
                Texture tex = new Texture(path);
                string absolute_path = path.Replace(folder_path, string.Empty).Trim();
                CacheDrawSpriteInstanceMap[absolute_path.ToLower()]= new SpriteInstanceGroup(DrawCallInstanceCountMax, absolute_path, tex);

                Log.Debug($"Loaded storyboard image file :{path}");
            });

            for (int i = 0; i < obj_list.Count; i++)
            {
                var obj = obj_list[i];
                if (!(obj is StoryboardAnimation animation))
                {
                    if (!CacheDrawSpriteInstanceMap.TryGetValue(obj.ImageFilePath.ToLower(), out obj.RenderGroup))
                    {
                        Log.Warn($"not found image:{obj.ImageFilePath}");
                    }
                }
                else
                {
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
                }
            }
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
            LinkedListNode<StoryBoardObject> LastAddNode=null;

            while (CurrentScanNode != null && CurrentScanNode.Value.FrameStartTime <= current_time/* && current_time <= CurrentScanNode.Value.FrameEndTime*/ )
            {
                if (current_time > CurrentScanNode.Value.FrameEndTime)
                {
                    CurrentScanNode = CurrentScanNode.Next;
                    continue;
                }

                Log.Debug($"[{current_time}]Add storyboard obj \"{CurrentScanNode.Value.ImageFilePath}\"");

                _UpdatingStoryBoard[CurrentScanNode.Value.layout].Add(CurrentScanNode.Value);
                
                LastAddNode = CurrentScanNode;

                CurrentScanNode = CurrentScanNode.Next;
            }

            if (LastAddNode!=null)
            {
                CurrentScanNode = LastAddNode.Next;
            }

            return /*isAdd*/LastAddNode!=null;
        }

        public void Update(float delay_time)
        {
            runTimer.Start();

            player.Tick();

            float current_time =player.CurrentFixedTime;

            bool hasAdded=Scan(current_time);
            
            foreach (var objs in _UpdatingStoryBoard.Values)
            {
                if (hasAdded)
                {
                    objs.Sort((a, b) => {
                        return a.Z - b.Z;
                    });
                }

                objs.ForEach/*AsParallel().ForAll*/(obj =>
                    {
                        if (current_time < obj.FrameStartTime || current_time > obj.FrameEndTime)
                        {
                            obj.markDone = true;
                        }
                        else
                        {
                            obj.markDone = false;
                            obj.Update(current_time);
                        }
                    }
                );
            }

            //remove unused objects
            foreach (var objs in _UpdatingStoryBoard.Values)
            {
                objs.RemoveAll((obj) =>
                {
                    if (!obj.markDone)
                    {
                        return false;
                    }

                    Log.Debug($"[{current_time}]remove object \"{obj.ImageFilePath}\"");

                    return true;
                });
            }

            #if DEBUG

            debug_instance.Update();

            #endif

            UpdateCastTime = runTimer.ElapsedMilliseconds;

            runTimer.Reset();
        }

        #region Storyboard Rendering

        public void PostDrawStoryBoard()
        {
            runTimer.Start();

            foreach (var layout_list in _UpdatingStoryBoard)
            {
                if (layout_list.Value.Count==0)
                {
                    continue;
                }

                PostDrawStoryBoardLayout(layout_list.Value);
            }

            RenderCastTime = runTimer.ElapsedMilliseconds;

            runTimer.Reset();
        }

        public void PostDrawStoryBoardLayout(List<StoryBoardObject> UpdatingStoryboardObjectList)
        {
            bool isEnable = GL.IsEnabled(EnableCap.DepthTest);
            
            DrawStoryBoards(UpdatingStoryboardObjectList);
        }

        private void DrawStoryBoards(List<StoryBoardObject> draw_list)
        {
            if (draw_list.Count == 0)
                return;
            
            SpriteInstanceGroup group = draw_list.First().RenderGroup;

            bool additive_trigger;
            ChangeAdditiveStatus(draw_list.First().IsAdditive);

            for (int i = 0; i < draw_list.Count; i++)
            {
                var obj = draw_list[i];

                if (obj.Color.w <= 0)
                    continue;//skip

                if (group!=obj.RenderGroup||additive_trigger!=obj.IsAdditive)
                {
                    group?.FlushDraw();

                    //应该是现在设置Blend否则Group自动渲染来不及钦定
                    ChangeAdditiveStatus(obj.IsAdditive);

                    group = obj.RenderGroup;
                }

                group?.PostRenderCommand(obj.Postion, obj.Z, obj.Rotate, obj.Scale,obj.Anchor, obj.Color,obj.IsVerticalFlip,obj.IsHorizonFlip);
            }
            
            if (group?.CurrentPostCount!=0)
                group?.FlushDraw();

            //恢复Blend
            ChangeAdditiveStatus(false);

            void ChangeAdditiveStatus(bool is_additive_blend)
            {
                additive_trigger = is_additive_blend;
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, additive_trigger ? BlendingFactorDest.One : BlendingFactorDest.OneMinusSrcAlpha);
            }
        }

        #endregion

        ~StoryBoardInstance()
        {
            player.Term();
        }
    }
}
