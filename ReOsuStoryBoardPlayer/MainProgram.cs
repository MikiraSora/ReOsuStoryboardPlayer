using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public class MainProgram
    {
        public static void Main(string[] argv)
        {
            string beatmap_folder=string.Empty;

            if (argv.Length == 0)
            {
                beatmap_folder = @"H:\SBTest\94790 Hatsuki Yura - Fuuga";
            }
            else
                beatmap_folder = argv[0];
            
            StoryBoardInstance instance = GetInstance(beatmap_folder);
            
            StoryboardWindow window = new StoryboardWindow(instance);

            Log.AbleDebugLog = false;

            window.Run();
        }

        static StoryBoardInstance GetInstance(string beatmap_folder)
        {
            if (string.IsNullOrWhiteSpace(beatmap_folder))
            {
                Exit("Please drag your beatmap folder to this program!");
            }

            if (!Directory.Exists(beatmap_folder))
            {
                Exit($"\"{beatmap_folder}\" not a folder!");
            }

            try
            {
                return new StoryBoardInstance(beatmap_folder);
            }
            catch (Exception e)
            {
               Exit($"Parse beatmap folder and load storyboard failed! {e.Message}");
            }

            return null;
        }

        static void Exit(string reason)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(reason);
            Console.ResetColor();
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
