using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

namespace ReOsuStoryBoardPlayer
{
    public static class Log
    {
        private static long _currentTime = 0;

        private static bool _ableLog = true;

        public static bool AbleLog
        {
            get => !Setting.MiniMode && _ableLog;
            set => _ableLog = value;
        }
        
        public static bool AbleDebugLog { get; set; } = false;

        private static ConsoleColor[] colors =
        {
            ConsoleColor.Yellow,ConsoleColor.Black,//Warn
            ConsoleColor.Red,ConsoleColor.Black,//Error
            ConsoleColor.White,ConsoleColor.Black,//Debug
            ConsoleColor.Green,ConsoleColor.Black//User
        };

        public enum LogLevel
        {
            Warn = 0,
            Error = 1,
            Debug = 2,
            User = 3,
            None = 4
        }

        internal static void Init()
        {
            _currentTime = Environment.TickCount;
            AbleLog = true;
        }

        private static string _getTimeStr()
        {
            long timePass = Environment.TickCount - _currentTime;
            long min = timePass / (60 * 1000), 
                sec = (timePass - min * (60 * 1000)) / 1000, 
                ms = timePass - min * 60000 - sec * 1000;

            return string.Format("{0:D2}:{1:D2}.{2:D3}", min, sec, ms);
        }

        private static string _buildLogMessage(string caller,string message, LogLevel level)
        {
            if (AbleDebugLog)
            {
                var result = new StackTrace().GetFrames().LastOrDefault(x=>x.GetMethod().Name==caller);

                if (result!=null)
                {
                    var method = result.GetMethod();
                    return string.Format("[{0}]{2}.{1}():\n>>{3}\n", _getTimeStr(), $"{method.DeclaringType.Name}.{method.Name}", level.ToString(), message);
                }
            }

            return string.Format("[{0}]{1}:{2}\n", _getTimeStr(), level.ToString(), message);
        }

        private static void _renderColor(ref string message, LogLevel level)
        {
            int index = (int)level;
            Console.ForegroundColor = colors[(index) * 2 + 0];
            Console.BackgroundColor = colors[(index) * 2 + 1];
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void _log(string caller, string message, LogLevel level)
        {
            if (!AbleLog)
                return;

            string output = _buildLogMessage(caller, message, level);

            _renderColor(ref output, level);
        }

        public static void User(string message, [CallerMemberName]string caller = "<Unknown Method>")
        {
            _log(caller,message, LogLevel.User);
        }

        public static void Warn(string message, [CallerMemberName]string caller = "<Unknown Method>")
        {
            _log(caller,message, LogLevel.Warn);
        }

        public static void Error(string message, [CallerMemberName]string caller = "<Unknown Method>")
        {
            _log(caller,message, LogLevel.Error);
        }

        public static void Debug(string message,[CallerMemberName]string caller= "<Unknown Method>")
        {
            if (!AbleDebugLog)
                return;
            _log(caller,message, LogLevel.Debug);
        }

        public static void Write(string message, [CallerMemberName]string caller = "<Unknown Method>")
        {
            _log(caller,message, LogLevel.None);
        }
    }
}