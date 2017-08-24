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

namespace ReOsuStoryBoardPlayer
{
    public class StoryBoardInstance
    {
        static readonly uint DrawCallInstanceCountMax = 50;
        
        LinkedList<StoryBoardObject> StoryboardObjectList;

        LinkedListNode<StoryBoardObject> CurrentScanNode;

        List<StoryBoardObject> DrawSplitList = new List<StoryBoardObject>();

        public Dictionary<Layout, List<StoryBoardObject>> _UpdatingStoryBoard = new Dictionary<Layout, List<StoryBoardObject>>();  

        public Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap = new Dictionary<string, SpriteInstanceGroup>();
        
        string osb_file_path = string.Empty, osu_file_path = string.Empty, audio_file_path = string.Empty,folder_path=string.Empty;

        public MusicPlayer player;

        public StoryBoardInstance(string folder_path)
        {
            if (!folder_path.EndsWith(@"\"))
            {
                folder_path += @"\";
            }

            this.folder_path = folder_path;

            StoryboardObjectList = new LinkedList<StoryBoardObject>();

            CurrentScanNode = StoryboardObjectList.First;

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

                    var match = Regex.Match(File.ReadAllText(path), @"AudioFilename:\s*(.+)");
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

            List<StoryBoardObject> temp_objs_list = new List<StoryBoardObject>();

            if ((!string.IsNullOrWhiteSpace(osb_file_path))&&File.Exists(osb_file_path))
            {
                List<StoryBoardObject> parse_osb_storyboard_objs = StoryBoardFileParser.ParseFromOsbFile(osb_file_path);

                if (parse_osb_storyboard_objs!=null)
                {
                    temp_objs_list.AddRange(parse_osb_storyboard_objs);
                }
            }
            
            //get objs from osu file
            List<StoryBoardObject> parse_osu_storyboard_objs = StoryBoardFileParser.ParseFromOsuFile(osu_file_path);
            //temp_objs_list.AddRange(parse_osu_storyboard_objs);

            temp_objs_list.Sort((a, b) => Math.Sign(a.FrameStartTime - b.FrameStartTime));

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
                CacheDrawSpriteInstanceMap.Add(absolute_path, new SpriteInstanceGroup(DrawCallInstanceCountMax, absolute_path, tex));

                Log.User($"Loaded storyboard image file :{path}");
            });
        }

        public void Start()
        {
            player.Play();
            CurrentScanNode = StoryboardObjectList.First;
        }

        public void Update()
        {
            uint current_time = player.CurrentPlayback;

            while (true)
            {
                if (CurrentScanNode == null)
                    break;

                if (CurrentScanNode.Value.FrameStartTime>=current_time)
                {
                    break;
                }

                Log.Debug($"[{current_time}]Add storyboard obj \"{CurrentScanNode.Value.ImageFilePath}\"");

                _UpdatingStoryBoard[CurrentScanNode.Value.layout].Add(CurrentScanNode.Value);

                CurrentScanNode = CurrentScanNode.Next;
            }
            
            foreach (var objs in _UpdatingStoryBoard.Values)
            {
                foreach (var obj in objs)
                {
                    StoryBoardObjectUpdate(obj, current_time);
                }
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
        }

        private void StoryBoardObjectUpdate(StoryBoardObject storyboard_obj,uint time)
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
            foreach (var layout_list in _UpdatingStoryBoard)
            {
                if (layout_list.Value.Count==0)
                {
                    continue;
                }

                PostDrawStoryBoardLayout(layout_list.Value);
            }
        }


        public void PostDrawStoryBoardLayout(List<StoryBoardObject> UpdatingStoryboardObjectList)
        {
            bool isEnable = GL.IsEnabled(EnableCap.DepthTest);

            GL.Disable(EnableCap.DepthTest);
            DrawNonTransparentStoryBoardObjects(UpdatingStoryboardObjectList);

            if (isEnable)
            {
                GL.Enable(EnableCap.DepthTest);
            }
        }

        private void DrawNonTransparentStoryBoardObjects(List<StoryBoardObject> nonTransparentsList)
        {
            if (nonTransparentsList.Count == 0)
                return;

            SpriteInstanceGroup group = CacheDrawSpriteInstanceMap[nonTransparentsList[0].ImageFilePath];

            foreach (var obj in nonTransparentsList)
            {
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
    }
}
