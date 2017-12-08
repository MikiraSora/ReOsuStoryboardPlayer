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

namespace ReOsuStoryBoardPlayer
{
    public class StoryBoardInstance
    {
        static readonly uint DrawCallInstanceCountMax = 50;
        
        LinkedList<StoryBoardObject> StoryboardObjectList;

        LinkedListNode<StoryBoardObject> CurrentScanNode;

        List<StoryBoardObject> DrawSplitList = new List<StoryBoardObject>();

        internal Dictionary<Layout, List<StoryBoardObject>> _UpdatingStoryBoard = new Dictionary<Layout, List<StoryBoardObject>>();

        internal Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap = new Dictionary<string, SpriteInstanceGroup>();

        internal string osb_file_path = string.Empty, osu_file_path = string.Empty, audio_file_path = string.Empty,folder_path=string.Empty;

        public MusicPlayer player;

        //internal float update_current_time;

        Stopwatch runTimer = new Stopwatch();

        public long UpdateCastTime { get; private set; }

        public long RenderCastTime { get; private set; }

        public StoryBoardInstance(string folder_path)
        {
            if (!folder_path.EndsWith(@"\"))
            {
                folder_path += @"\";
            }

            this.folder_path = folder_path;

            StoryboardObjectList = new LinkedList<StoryBoardObject>();

            CurrentScanNode = StoryboardObjectList.First;

            int audioLeadIn = 0;

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
                    var match = Regex.Match(content, @"AudioFilename:\s*(.+)");

                    audioLeadIn = int.Parse(Regex.Match(content, @"AudioLeadIn:\s*(.+)").Groups[1].Value.Replace("\r", string.Empty));
                    if (true)
                    {
                        audio_file_path =folder_path + match.Groups[1].Value.Replace("\r",string.Empty);
                        Log.User($"audio file path={audio_file_path}");
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

            //BuildCacheDrawSpriteBatch();

            #region Load and Parse osb/osu file

            List<StoryBoardObject> temp_objs_list = new List<StoryBoardObject>(), parse_osb_storyboard_objs=new List<StoryBoardObject>();

            int z_order = 0;

            //get objs from osu file
            List<StoryBoardObject> parse_osu_storyboard_objs = StoryBoardFileParser.ParseFromOsuFile(osu_file_path,ref z_order);

            if ((!string.IsNullOrWhiteSpace(osb_file_path))&&File.Exists(osb_file_path))
            {
                parse_osb_storyboard_objs = StoryBoardFileParser.ParseFromOsbFile(osb_file_path,ref z_order);
            }
            
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

            StringBuilder builder = new StringBuilder();
            foreach (var sprite in StoryboardObjectList)
            {
                builder.AppendLine($"{sprite.ImageFilePath}[{sprite.FrameStartTime} - {sprite.FrameEndTime}]");
            }

            #if DEBUG

            InitDebugControllerWindow();

            #endif
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
            List<String> pic_list = new List<string>();
            pic_list.AddRange(Directory.EnumerateFiles(folder_path, "*.png", SearchOption.AllDirectories));
            pic_list.AddRange(Directory.EnumerateFiles(folder_path, "*.jpg", SearchOption.AllDirectories));

            pic_list.ForEach((path)=>
            {
                Texture tex = new Texture(path);
                string absolute_path = path.Replace(folder_path, string.Empty).Trim();
                CacheDrawSpriteInstanceMap.Add(absolute_path.ToLower(), new SpriteInstanceGroup(DrawCallInstanceCountMax, absolute_path, tex));

                Log.User($"Loaded storyboard image file :{path}");
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

            //debug
            player.Pause();
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
                        obj.Update(current_time);
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

            CallUpdateDebugControllerWindowInfo();

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
            
            SpriteInstanceGroup group = CacheDrawSpriteInstanceMap[draw_list[0].ImageFilePath];

            bool additive_trigger = draw_list.First().IsAdditive;

            foreach (var obj in draw_list)
            {
                if (obj.Color.w <= 0)
                    continue;//skip

                if (group.ImagePath!=obj.ImageFilePath||additive_trigger!=obj.IsAdditive)
                {
                    PostDraw();
                    additive_trigger = obj.IsAdditive;
                    group = CacheDrawSpriteInstanceMap[obj.ImageFilePath];
                }
                group.PostRenderCommand(obj.Postion, obj.Z, obj.Rotate, obj.Scale,obj.Anchor, obj.Color);
            }

            if (group.CurrentPostCount!=0)
            {
                PostDraw();
            } 

            void PostDraw()
            {
                if (additive_trigger)
                {
                    GL.BlendFunc(BlendingFactorSrc.SrcColor, BlendingFactorDest.DstColor);
                }
                else
                {
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                }
                
                group.FlushDraw();
            }
        }

        #endregion

        #region Debug Controller

        #if DEBUG

        DebugController.ControllerWindow ControllerWindow;

        string debug_break_storyboard_image;

        Event debug_break_event;
        
        public void DumpCurrentStoryboardStatus()
        {
            foreach (var layout in _UpdatingStoryBoard)
            {
                Log.User($"Dump Layout:{layout.Key.ToString()}");
                foreach (var obj in layout.Value)
                {
                    Log.User($"\"{obj.ImageFilePath}\" \\ Z = {obj.Z} \\ {obj.FrameStartTime} ~ {obj.FrameEndTime} \nPosition={obj.Postion} \\ Rotate = {obj.Rotate} \\ Scale = {obj.Scale} \n Color = {obj.Color} \\ Anchor : {obj.Anchor} \n -----------------------");
                }
            }
        }

        public void CreateBreakpointInCommandExecuting(string break_storyboard_image,Event break_event)
        {
            this.debug_break_event = break_event;
            this.debug_break_storyboard_image = break_storyboard_image.Trim().Replace("/","\\");
            Flush();
        }

        public void ClearBreakpoint()
        {
            this.debug_break_storyboard_image = string.Empty;
        }

        public void CallUpdateDebugControllerWindowInfo() => ControllerWindow.UpdateInfo();

        public void InitDebugControllerWindow()
        {
            ControllerWindow = new DebugController.ControllerWindow(this);
            ControllerWindow.Show();
            ControllerWindow.progressBar1.Maximum = (int)player.Length;
        }

#endif

        #endregion

        ~StoryBoardInstance()
        {
            player.Term();
        }
    }
}
