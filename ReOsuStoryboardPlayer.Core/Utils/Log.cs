using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ReOsuStoryboardPlayer.Core.Utils
{
    public static class Log
    {
        public delegate void LogDelegate(string caller, string message, LogLevel level);

        public enum LogLevel
        {
            Warn = 0,
            Error = 1,
            Debug = 2,
            User = 3,
            None = 4
        }

        private static readonly long _currentTime;

        private static readonly ConsoleColor[] colors =
        {
            ConsoleColor.Yellow, ConsoleColor.Black, //Warn
            ConsoleColor.Yellow, ConsoleColor.Red, //Error
            ConsoleColor.White, ConsoleColor.Black, //Debug
            ConsoleColor.Green, ConsoleColor.Black //User
        };

        static Log()
        {
            _currentTime = Environment.TickCount;
        }

        public static LogDelegate LogImplement { get; set; } = defaultLogImpl;

        public static bool AbleDebugLog { get; set; } = false;

        private static string _getTimeStr()
        {
            var timePass = Environment.TickCount - _currentTime;
            long min = timePass / (60 * 1000),
                sec = (timePass - min * (60 * 1000)) / 1000,
                ms = timePass - min * 60000 - sec * 1000;

            return string.Format("{0:D2}:{1:D2}.{2:D3}", min, sec, ms);
        }

        private static string buildLogMessage(string caller, string message, LogLevel level)
        {
            if (AbleDebugLog)
            {
                var result = new StackTrace().GetFrames()?.LastOrDefault(x => x.GetMethod().Name == caller);

                if (result != null)
                {
                    var method = result.GetMethod();
                    return string.Format("[{0}]{2}.{1}():\n>>{3}\n", _getTimeStr(),
                        $"{method?.DeclaringType?.Name}.{method?.Name}", level.ToString(), message);
                }
            }

            return string.Format("[{0}]{1}:{2}\n", _getTimeStr(), level.ToString(), message);
        }

        private static void _renderColor(string message, LogLevel level)
        {
            var index = (int) level;
            Console.ForegroundColor = colors[index * 2 + 0];
            Console.BackgroundColor = colors[index * 2 + 1];
            System.Diagnostics.Debug.Print(message);
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void defaultLogImpl(string caller, string message, LogLevel level)
        {
            var output = buildLogMessage(caller, message, level);
            _renderColor(output, level);
        }

        private static void _log(string caller, string message, LogLevel level)
        {
            if (!Setting.AllowLog)
                return;

            LogImplement?.Invoke(caller, message, level);
        }

        public static void User(string message, [CallerMemberName] string caller = "<Unknown Method>")
        {
            _log(caller, message, LogLevel.User);
        }

        public static void Warn(string message, [CallerMemberName] string caller = "<Unknown Method>")
        {
            _log(caller, message, LogLevel.Warn);
        }

        public static void Error(string message, [CallerMemberName] string caller = "<Unknown Method>")
        {
            _log(caller, message, LogLevel.Error);
        }

        public static void Debug(string message, [CallerMemberName] string caller = "<Unknown Method>")
        {
            if (!AbleDebugLog)
                return;
            _log(caller, message, LogLevel.Debug);
        }

        public static void Write(string message, [CallerMemberName] string caller = "<Unknown Method>")
        {
            _log(caller, message, LogLevel.None);
        }
    }
}