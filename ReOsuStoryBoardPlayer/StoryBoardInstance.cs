using SimpleRenderFramework;
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

        internal float update_current_time;

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

            BuildCacheDrawSpriteBatch();

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
            
            update_current_time = new_time;
        }

        private void BuildCacheDrawSpriteBatch()
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
            update_current_time = 0;
        }

        public void Flush()
        {
            foreach (var pair in _UpdatingStoryBoard)
            {
                pair.Value.Clear();

            }
            update_current_time = 0;

            CurrentScanNode = StoryboardObjectList.First;

            ScanPrev(player.CurrentPlayback);

            //debug
            player.Pause();
        }

        private bool ScanNext(float current_time)
        {
            bool isAdd = false;

            while (true)
            {
                if (CurrentScanNode == null || CurrentScanNode.Value.FrameStartTime >= current_time)
                    break;

                Log.Debug($"[{current_time}]Add storyboard obj \"{CurrentScanNode.Value.ImageFilePath}\"");

                _UpdatingStoryBoard[CurrentScanNode.Value.layout].Add(CurrentScanNode.Value);

                isAdd = true;

                CurrentScanNode = CurrentScanNode.Next;
            }

            return isAdd;
        }
        
        private bool ScanPrev(float current_time)
        {
            bool isAdd = false;

            LinkedListNode<StoryBoardObject> CurrentScanNode=StoryboardObjectList.First;

            while (true)
            {
                if (CurrentScanNode == null || CurrentScanNode.Value.FrameEndTime < current_time)
                    break;

                //start_time<= current_time <=end_time
                if (current_time<=CurrentScanNode.Value.FrameEndTime&&CurrentScanNode.Value.FrameStartTime<=current_time)
                {

                    Log.User($"[ScanPrev][{current_time} : {CurrentScanNode.Value.FrameStartTime} ~ {CurrentScanNode.Value.FrameEndTime}]Add storyboard obj \"{CurrentScanNode.Value.ImageFilePath}\"");

                    _UpdatingStoryBoard[CurrentScanNode.Value.layout].Add(CurrentScanNode.Value);

                    isAdd = true;
                }
                
                CurrentScanNode = CurrentScanNode.Next;
            }

            return isAdd;
        }
        
        public void Update(float delay_time)
        {
            runTimer.Start();

            if (player.IsPlaying)
            {
                update_current_time += delay_time;

                if (Math.Abs(update_current_time-player.CurrentPlayback)>22)
                {
                    /*
                     * 现存BUG，update累计时间太慢以至于播放器的时间同步
                     */
                    //force
                    update_current_time = player.CurrentPlayback;
                }
            }

            float current_time = update_current_time;

            bool hasAdded = ScanNext(current_time);
            
            foreach (var objs in _UpdatingStoryBoard.Values)
            {
                if (hasAdded)
                {
                    objs.Sort((a, b) => {
                        /*
                         * 咕咕咕
                         */
                        /*
                        if (a.FrameStartTime != b.FrameStartTime)
                        {
                            return a.FrameStartTime - b.FrameStartTime;
                        }
                        */
                        return a.Z - b.Z;
                    });
                }

                objs.ForEach(obj =>
                    {
                        StoryBoardObjectUpdate(obj, current_time);
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

        private void StoryBoardObjectUpdate(StoryBoardObject storyboard_obj,float time)
        {
            if (time > storyboard_obj.FrameEndTime)
            {
                storyboard_obj.markDone = true;
                return;
            }

            foreach (var command_list in storyboard_obj.CommandMap.Values)
            {
                Command command=null;
                if (time<command_list[0].StartTime)
                {
                    //早于开始前
                    command = command_list[0];
                }
                else if (time>command_list[command_list.Count-1].EndTime)
                {
                    //迟于结束后
                    command = command_list[command_list.Count - 1];
                }

                #if DEBUG

                if (storyboard_obj.ImageFilePath==debug_break_storyboard_image&&command?.CommandEventType==debug_break_event)
                {
                    player.Pause();
                    Debugger.Break();   
                }

                #endif

                if (command!=null)
                {
                    CommandExecutor.DispatchCommandExecute(storyboard_obj, time, command);
                    continue;
                }

                foreach (var cmd in command_list)
                {
                    if (time < cmd.StartTime|| time > cmd.EndTime)
                        continue;
                    CommandExecutor.DispatchCommandExecute(storyboard_obj, time, cmd);
                }
            }
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

            GL.Disable(EnableCap.DepthTest);
            DrawStoryBoards(UpdatingStoryboardObjectList);

            if (isEnable)
            {
                GL.Enable(EnableCap.DepthTest);
            }
        }

        private void DrawStoryBoards(List<StoryBoardObject> draw_list)
        {
            if (draw_list.Count == 0)
                return;
            
            SpriteInstanceGroup group = CacheDrawSpriteInstanceMap[draw_list[0].ImageFilePath];

            foreach (var obj in draw_list)
            {
                if (obj.Color.w <= 0)
                    continue;//skip

                if (group.ImagePath!=obj.ImageFilePath)
                {
                    //draw immediatly and switch to new group
                    group.FlushDraw();
                    group = CacheDrawSpriteInstanceMap[obj.ImageFilePath];
                }
                group.PostRenderCommand(obj.Postion, obj.Z, obj.Rotate, obj.Scale,obj.Anchor, obj.Color);
            }

            if (group.CurrentPostCount!=0)
            {
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
    }
}
