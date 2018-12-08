using ReOsuStoryBoardPlayer.ProgramCommandParser;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.CLIController
{
    public class CLIControllerDebugger : DebuggerBase
    {
        Thread thread;
        CommandParser parser = new CommandParser(new ParamParserV2('-', '\"', '\''));

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

                var cmd = parser.Parse(command, out var cmdName);

                switch (cmdName)
                {
                    case "file":
                        //load a file
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

                        ExecutorSync.PostTask(() => MusicPlayerManager.ActivityPlayer.Jump(num)).Wait();
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
                        if (rstr==null||!rstr.Contains("x")) break;
                        var d = rstr.Split('x');
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
