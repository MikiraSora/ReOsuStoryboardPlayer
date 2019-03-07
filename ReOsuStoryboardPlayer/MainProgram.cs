using ReOsuStoryboardPlayer.Core.Parser;
using ReOsuStoryboardPlayer.Core.Parser.Collection;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayer.Core.Parser.Stream;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Tools;
using ReOsuStoryboardPlayer.Tools.DefaultTools.AutoTriggerContoller;
using ReOsuStoryboardPlayer.Tools.DefaultTools.CLIController;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.OutputEncoding;
using ReOsuStoryboardPlayer.OutputEncoding.Kernel;
using ReOsuStoryboardPlayer.OutputEncoding.Player;
using ReOsuStoryboardPlayer.Parser;
using ReOsuStoryboardPlayer.Player;
using ReOsuStoryboardPlayer.ProgramCommandParser;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ReOsuStoryboardPlayer.Utils;
using ReOsuStoryBoardPlayer.OutputEncoding.Graphics.PostProcess;

namespace ReOsuStoryboardPlayer
{
    public class MainProgram
    {
        #region Console Close Event

        internal delegate bool ControlCtrlDelegate(int CtrlType);
        [DllImport("kernel32.dll")]
        internal static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);

        #endregion

        public static void Main(string[] argv)
        {
            Environment.CurrentDirectory=System.AppDomain.CurrentDomain.BaseDirectory;

            SetConsoleCtrlHandler(type => {
                Exit();
                return true;
            },false);

            PlayerSetting.Init();

            var args = ParseProgramCommands(argv, out var beatmap_folder);

            EnvironmentHelper.SetupEnvironment();

            //Update check
            if (PlayerSetting.EnableUpdateCheck)
                ProgramUpdater.UpdateCheck();

            //clean temp folder if updated just now.
            ProgramUpdater.CleanTemp();

            PlayerSetting.PrintSettings();

            //init window
            StoryboardWindow window = new StoryboardWindow(PlayerSetting.Width, PlayerSetting.Height);

            Log.User($"Start to parse folder :{beatmap_folder}");

            if (Directory.Exists(beatmap_folder))
            {
                var info = BeatmapFolderInfo.Parse(beatmap_folder, args);
                var instance = StoryboardInstance.Load(info);

                window.LoadStoryboardInstance(instance);

                var player = new MusicPlayer();
                player.Load(info.audio_file_path);
                MusicPlayerManager.ApplyPlayer(player);

                var auto_trigger = ToolManager.GetOrCreateTool<AutoTrigger>();
                auto_trigger.Load(info);
                auto_trigger.Trim();
            }
            else
            {
                Exit($"You have to select a beatmap folder which contains storyboard to play");
            }

            if (PlayerSetting.EncodingEnvironment)
            {
                //init encoding environment
                var encoding_opt = new EncoderOption(args);
                EncodingKernel encoding_kernel = new EncodingKernel(encoding_opt);
                EncodingProcessPlayer encoding_player = new EncodingProcessPlayer(MusicPlayerManager.ActivityPlayer.Length, encoding_opt.FPS);
                MusicPlayerManager.ActivityPlayer.Pause();
                MusicPlayerManager.ApplyPlayer(encoding_player);
                ToolManager.AddTool(encoding_kernel);
                encoding_kernel.Start();
            }

            MusicPlayerManager.ActivityPlayer?.Play();

            window.Run();
        }

        #region ProgramCommands

        private static Parameters ParseProgramCommands(string[] argv, out string beatmap_folder)
        {
            beatmap_folder=@"G:\SBTest\695053 BlackY - Double Pendulum";

            var sb = new ArgParser(new ParamParserV2('-', '\"', '\''));
            var args = sb.Parse(argv);

            if (args!=null)
            {
                if (args.Switches.Any(k => k=="help"))
                {
                    Console.WriteLine("please visit here: https://github.com/MikiraSora/OsuStoryboardPlayer/wiki/Program-command-options");
                    Exit("");
                }

                if (args.TryGetSwitch("program_update"))
                {
                    int id = 0;
                    var x = int.TryParse(args.FreeArgs.FirstOrDefault(), out id);

                    ProgramUpdater.ApplyUpdate(x ? id : 0);
                }

                if (args.FreeArgs!=null)
                    beatmap_folder=args.FreeArgs.FirstOrDefault()??beatmap_folder;

                if (args.TryGetArg(out var valW, "width", "w"))
                    PlayerSetting.FrameWidth = PlayerSetting.Width = int.Parse(valW);

                if (args.TryGetArg(out var valH, "height", "h"))
                    PlayerSetting.FrameHeight = PlayerSetting.Height = int.Parse(valH);

                if (args.TryGetArg(out var valFW, "freame_width", "fw"))
                    PlayerSetting.FrameWidth = int.Parse(valFW);

                if (args.TryGetArg(out var valFH, "freame_height", "fh"))
                    PlayerSetting.FrameHeight = int.Parse(valFH);

                if (args.TryGetArg(out var draw_count, "multi_instance_render", "mtr"))
                    PlayerSetting.DrawCallInstanceCountMax = int.Parse(draw_count);

                if (args.TryGetArg(out var folder, "folder", "f"))
                    beatmap_folder=folder;

                if (args.TryGetArg(out var p_update_limit, "parallel_update_limit", "pu"))
                    PlayerSetting.ParallelUpdateObjectsLimitCount=p_update_limit.ToInt();

                if (args.TryGetArg(out var update_thread_count, "update_thread_count", "ut"))
                    PlayerSetting.UpdateThreadCount=update_thread_count.ToInt();

                if (args.TryGetArg(out var max_fps, "fps"))
                    PlayerSetting.MaxFPS=max_fps.ToInt();

                if (args.TryGetArg(out var ssaa, "ssaa"))
                    PlayerSetting.SsaaLevel=ssaa.ToInt();

                if (args.Switches.Any(k => k=="enable_timestamp"))
                    PlayerSetting.EnableTimestamp=true;

                if (args.Switches.Any(k => k=="full_screen"))
                    PlayerSetting.EnableFullScreen=true;

                if (args.Switches.Any(k => k=="borderless"))
                    PlayerSetting.EnableBorderless=true;

                if (args.Switches.Any(k => k=="enable_loop_expand"))
                    PlayerSetting.EnableLoopCommandExpand=true;

                if (args.Switches.Any(k => k=="mini"))
                    PlayerSetting.MiniMode=true;

                if (args.Switches.Any(k => k=="disable_split"))
                    PlayerSetting.EnableSplitMoveScaleCommand=false;

                if (args.Switches.Any(k => k=="fun_reverse_easing"))
                    PlayerSetting.FunReverseEasing=true;

                if (args.TryGetArg(out var ol,"optimzer_level" ,"o"))
                    PlayerSetting.StoryboardObjectOptimzeLevel=ol.ToInt();

                if (args.TryGetSwitch("disable_update_check"))
                    PlayerSetting.EnableUpdateCheck=false;

                if (args.Switches.Any(k => k=="disable_hp_fps_limit"))
                    PlayerSetting.EnableHighPrecisionFPSLimit=false;

                if (args.Switches.Any(k => k=="debug"))
                    PlayerSetting.DebugMode=true;

                if (args.Switches.Any(k => k=="cli"))
                    ToolManager.GetOrCreateTool<CLIControllerDebugger>();

                //额外功能 - 提取解析好变量的文本
                if (args.TryGetArg(out var parse_type, "parse"))
                {
                    var parse_osb = parse_type=="osb";
                    args.TryGetArg(out var output_path, "parse_output");
                    SerializeDecodeStoryboardContent(beatmap_folder, parse_osb, output_path);
                }

                //额外功能 - 输出优化提示
                if (args.TryGetSwitch("show_profile_suggest"))
                    PlayerSetting.ShowProfileSuggest=true;

                if (args.Switches.Any(k => k=="encode"))
                    PlayerSetting.EncodingEnvironment=true;
            }

            return args;
        }

        //将解析好的内容再序列化成osb文件格式的文本内容
        private static void SerializeDecodeStoryboardContent(string beatmap_folder, bool parse_osb, string output_path)
        {
            try
            {
                var info = BeatmapFolderInfo.Parse(beatmap_folder, null);
                var input_file = parse_osb ? info.osb_file_path : info.osu_file_path;
                output_path=string.IsNullOrWhiteSpace(output_path) ? input_file+".parse_output" : output_path;

                Log.User($"Start serialize {input_file} ....");
                using (var writer = new StreamWriter(File.OpenWrite(output_path)))
                {
                    writer.WriteLine($"[{Section.Events.ToString()}]");

                    OsuFileReader reader = new OsuFileReader(input_file);

                    VariableCollection collection = new VariableCollection(new VariableReader(reader).EnumValues());

                    SectionReader section = new SectionReader(Section.Events, reader);

                    EventReader event_section = new EventReader(reader, collection);

                    foreach (var line in section.EnumValues())
                    {
                        var decode_line = event_section.LineProcessVariable(line);
                        writer.WriteLine(decode_line);
                    }
                }

                Log.User("Serialize successfully! it output to "+output_path);
                Exit("Serialize successfully! it output to "+output_path);
            }
            catch (Exception e)
            {
                Log.Error("Serialize failed!"+e.Message);
                Exit("Serialize failed!"+e.Message);
            }
        }

        #endregion ProgramCommands

        //程序退出
        public static void Exit(string error_reason = "")
        {
            try
            {
                ToolManager.Close();
                StoryboardWindow.CurrentWindow?.Close();

                if ((!PlayerSetting.MiniMode)&&(!string.IsNullOrWhiteSpace(error_reason)))
                {
                    Console.BackgroundColor=ConsoleColor.Red;
                    Console.ForegroundColor=ConsoleColor.Yellow;
                    Console.WriteLine(error_reason);
                    Console.ResetColor();
                    Console.ReadKey();

                    Environment.Exit(2857);//2q1
                }

            }
            catch (Exception e)
            {
                Log.Warn("Can't clean resource and others before exit :"+e.Message);
            }

            Environment.Exit(0);
        }
    }
}