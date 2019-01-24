using ReOsuStoryBoardPlayer.ProgramCommandParser;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Player;
using ReOsuStoryBoardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Utils;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.CLIController
{
    public class CLIControllerDebugger : DebuggerBase
    {
        Thread thread;
        CommandParser parser = new CommandParser(new ParamParserV2('-', '\"', '\''));
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
                    case "file":
                        var folder_path = cmd.FreeArgs.FirstOrDefault();
                        if ((!string.IsNullOrWhiteSpace(folder_path))&&Directory.Exists(folder_path))
                        {
                            var info = BeatmapFolderInfo.Parse(folder_path,null);
                            StoryboardPlayerHelper.PlayStoryboard(info);
                        }
                        break;
                    case "play":
                        MusicPlayerManager.ActivityPlayer.Play();
                        break;
                    case "pause":
                        MusicPlayerManager.ActivityPlayer.Pause();
                        break;
                    case "jump":
                        var str = cmd.FreeArgs.FirstOrDefault();
                        if (str==null) break;
                        var num = uint.Parse(str);

                        ExecutorSync.PostTask(() => MusicPlayerManager.ActivityPlayer.Jump(num,true)).Wait();
                        break;
                    case "exit":
                    case "quit":
                        StoryboardWindow.CurrentWindow.Close();
                        break;
                    case "moveTo": //x,y坐标
                        throw new NotImplementedException();
                    case "scale": //1.0为基准这样
                                  //case "sizeTo": //或者具体到分辨率
                        throw new NotImplementedException();
                    case "window_resize":
                        var rstr = cmd.FreeArgs.FirstOrDefault();
                        if (rstr==null||!rstr.Any(x=> size_split.Contains(x))) break;
                        var d = rstr.Split(size_split);
                        var nw = d[0].ToInt();
                        var nh = d[1].ToInt();
                        StoryboardWindow.CurrentWindow.Width=nw;
                        StoryboardWindow.CurrentWindow.Height=nh;
                        break;
                    case "volume":
                        MusicPlayerManager.ActivityPlayer.Volume=cmd.FreeArgs.FirstOrDefault()?.ToSigle()??MusicPlayerManager.ActivityPlayer.Volume;
                        break;
                    case "playback_speed":
                        MusicPlayerManager.ActivityPlayer.PlaybackSpeed=cmd.FreeArgs.FirstOrDefault()?.ToSigle()??MusicPlayerManager.ActivityPlayer.PlaybackSpeed;
                        break;
                    case "fullscreen":
                        var fsw = cmd.FreeArgs.FirstOrDefault()??string.Empty;
                        var window = StoryboardWindow.CurrentWindow;
                        if (string.IsNullOrWhiteSpace(fsw))
                            window.SwitchFullscreen(!window.IsFullScreen);
                        else
                            window.SwitchFullscreen(bool.Parse(fsw));
                        break;
                    case "borderless":
                        var bsw = cmd.FreeArgs.FirstOrDefault()??string.Empty;
                        window = StoryboardWindow.CurrentWindow;
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
