﻿using System;
using System.IO;

namespace ReOsuStoryBoardPlayer
{
    public class MainProgram
    {
        public static void Main(string[] argv)
        {
#if DEBUG
            Log.AbleDebugLog = true;
#else
            Log.AbleDebugLog = false;
#endif

            string beatmap_folder = string.Empty;

            if (argv.Length == 0)
            {
                beatmap_folder =@"G:\SBTest\46216 Watarirouka Hashiritai7 - Valentine Kiss";
            }
            else
                beatmap_folder = argv[0];

            StoryBoardInstance instance = GetInstance(beatmap_folder);

            StoryboardWindow window = new StoryboardWindow(747, 960);

            window.LoadStoryboardInstance(instance);

            window.Run();
        }

        private static StoryBoardInstance GetInstance(string beatmap_folder)
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

        private static void Exit(string reason)
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