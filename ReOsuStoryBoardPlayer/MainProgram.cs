using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Player;
using System;
using ReOsuStoryBoardPlayer.Commands;
using System.Linq;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.CLIController;
using ReOsuStoryBoardPlayer.Parser.Extension;
using System.Runtime.InteropServices;

namespace ReOsuStoryBoardPlayer
{
    public class MainProgram
    {
        public static void Main(string[] argv)
        {
            Setting.Init();

            ParseProgramCommands(argv, out var w, out var h, out var beatmap_folder);

            var info = BeatmapFolderInfo.Parse(beatmap_folder);

            //init audio
            var player = new MusicPlayer();
            player.Load(info.audio_file_path);

            MusicPlayerManager.ApplyPlayer(player);

            //load storyboard objects
            var instance = new StoryBoardInstance(info);

            //init window
            StoryboardWindow window = new StoryboardWindow(w, h);
            window.LoadStoryboardInstance(instance);
            //init control panel and debuggers

            #region CLI Control

            //Log.AbleDebugLog = false;
            if (Setting.MiniMode)
            {
                DebuggerManager.AddDebugger(new CLIControllerDebugger());
            }
            else
            {
#if DEBUG
                DebuggerHelper.SetupDebugEnvironment();
#else
                DebuggerHelper.SetupReleaseEnvironment();
#endif
            }

            #endregion

            player.Play();
            window.Run();
        }

        private static void ParseProgramCommands(string[] argv, out int w, out int h,out string beatmap_folder)
        {
            //default 
            w=1600;
            h=900;
            beatmap_folder = @"G:\SBTest\237977 marina - Towa yori Towa ni";

            var sb = new ArgParser(new ParamParserV2('-', '\"', '\''));
            var args = sb.Parse(argv);

            if (args!=null)
            {
                if (args.FreeArgs!=null)
                    beatmap_folder=args.FreeArgs.FirstOrDefault()??beatmap_folder;
                
                if (args.TryGetArg(out var valW, "width", "w"))
                    w=int.Parse(valW);

                if (args.TryGetArg(out var valH, "height", "h"))
                    h=int.Parse(valH);

                if (args.TryGetArg(out var folder, "folder","f"))
                    beatmap_folder=folder;
                
                if (args.TryGetArg(out var p_update_limit, "-parallel_update_limit","-pu"))
                    Setting.ParallelUpdateObjectsLimitCount=p_update_limit.ToInt();
                
                if (args.TryGetArg(out var p_parse_limit, "-parallel_parse_limit", "-pp"))
                    Setting.ParallelParseCommandLimitCount=p_parse_limit.ToInt();

                Setting.MiniMode=args.Switches.Any(k => k=="mini");
                Setting.EnableSplitMoveScaleCommand=args.Switches.Any(k => k=="enable_split");
                Setting.EnableRuntimeOptimzeObjects=args.Switches.Any(k => k=="enable_runtime_optimze");
            }
        }

        private static void Exit(string reason)
        {
            if (!Setting.MiniMode)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(reason);
                Console.ResetColor();
                Console.ReadKey();
            }

            Environment.Exit(0);
        }
    }
}