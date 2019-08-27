using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Parser;
using ReOsuStoryboardPlayer.Player;
using ReOsuStoryboardPlayer.ProgramCommandParser;
using ReOsuStoryboardPlayer.Utils;
using ReOsuStoryBoardPlayer.Parser;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace ReOsuStoryboardPlayer.Tools.DefaultTools.CLIController
{
    public class CLIControllerDebugger : ToolBase
    {
        private Thread thread;
        private CommandParser parser = new CommandParser(new ParamParserV2('-', '\"', '\''));
        private static char[] size_split = new[] { 'x', '*' };

        public override void Init()
        {
            thread=new Thread(ConsoleReader);
            thread.Name="CLIControllerDebugger Console Reader";
            thread.Start();
        }

        private void ConsoleReader()
        {
            while (true)
            {
                var line = Console.ReadLine();

                ExecuteCommand(line);
            }
        }

        public void ExecuteCommand(string command)
        {
            //考虑到还有其他玩意用CLI接口
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(command))
                    return;

                var cmd = parser.Parse(command, out var cmdName)
                    ??new Parameters();//default empty

                switch (cmdName)
                {
                    /*
                        播放指定的文件夹的sb
                        >file "G:\osu!\Songs\747313 Shimotsuki Haruka - Liblume"   
                     */
                    case "file":
                        var folder_path = cmd.FreeArgs.FirstOrDefault();
                        if ((!string.IsNullOrWhiteSpace(folder_path))&&Directory.Exists(folder_path))
                        {
                            var info = BeatmapFolderInfoEx.Parse(folder_path,null);
                            StoryboardPlayerHelper.PlayStoryboard(info);
                        }
                        break;

                    /*
                     开始/恢复播放
                     >play                      
                    */
                    case "play":
                        MusicPlayerManager.ActivityPlayer.Play();
                        break;

                    /*
                     暂停播放
                     >pause                      
                    */
                    case "pause":
                        MusicPlayerManager.ActivityPlayer.Pause();
                        break;

                    /*
                     跳到15000ms处的画面
                     >jump 15000                      
                    */
                    case "jump":
                        var str = cmd.FreeArgs.FirstOrDefault();
                        if (str==null) break;
                        var num = uint.Parse(str);

                        ExecutorSync.PostTask(() => MusicPlayerManager.ActivityPlayer.Jump(num, true)).Wait();
                        break;

                    /*
                    QUITZERA,关闭播放器
                    >exit
                    或者
                    >quit                      
                    */
                    case "exit":
                    case "quit":
                        StoryboardWindow.CurrentWindow.Close();
                        break;

                    case "moveTo": //x,y坐标
                        throw new NotImplementedException();
                    case "scale": //1.0为基准这样
                                  //case "sizeTo": //或者具体到分辨率
                        throw new NotImplementedException();

                    /*
                     改变窗口分辨率(不是渲染大小)
                     >window_resize 1600x900   
                     或者
                     >window_resize 1600*900                     
                    */
                    case "window_resize":
                        var rstr = cmd.FreeArgs.FirstOrDefault();
                        if (rstr==null||!rstr.Any(x => size_split.Contains(x))) break;
                        var d = rstr.Split(size_split);
                        var nw = d[0].ToInt();
                        var nh = d[1].ToInt();
                        StoryboardWindow.CurrentWindow.Width=nw;
                        StoryboardWindow.CurrentWindow.Height=nh;
                        break;

                    /*
                     改变播放音乐音量(0~1)
                     >volume 0.5                 
                    */
                    case "volume":
                        MusicPlayerManager.ActivityPlayer.Volume=cmd.FreeArgs.FirstOrDefault()?.ToSigle()??MusicPlayerManager.ActivityPlayer.Volume;
                        break;

                    /*
                     改变 音乐/画面 播放速率
                     >playback_speed 2 
                     >playback_speed 0.5
                    */
                    case "playback_speed":
                        MusicPlayerManager.ActivityPlayer.PlaybackSpeed=cmd.FreeArgs.FirstOrDefault()?.ToSigle()??MusicPlayerManager.ActivityPlayer.PlaybackSpeed;
                        break;

                    /*
                     切换全屏播放
                     >fullscreen
                     >fullscreen true 
                     >fullscreen false 
                    */
                    case "fullscreen":
                        var fsw = cmd.FreeArgs.FirstOrDefault()??string.Empty;
                        var window = StoryboardWindow.CurrentWindow;
                        if (string.IsNullOrWhiteSpace(fsw))
                            window.SwitchFullscreen(!window.IsFullScreen);
                        else
                            window.SwitchFullscreen(bool.Parse(fsw));
                        break;

                    /*
                     切换无边窗播放
                     >borderless
                     >borderless true
                     >borderless false 
                    */
                    case "borderless":
                        var bsw = cmd.FreeArgs.FirstOrDefault()??string.Empty;
                        window=StoryboardWindow.CurrentWindow;
                        if (string.IsNullOrWhiteSpace(bsw))
                            window.ApplyBorderless(!window.IsBorderless);
                        else
                            window.ApplyBorderless(bool.Parse(bsw));
                        break;

                    default:
                        break;
                }
            }
        }

        public override void Term()
        {
            if (thread!=null)
            {
                try
                {
                    thread.Abort();
                }
                catch (Exception e)
                {
                    Log.Warn($"Can't stop console reader thread :{e.Message}");
                }
            }
        }

        public override void Update()
        {
            //咕咕
        }
    }
}