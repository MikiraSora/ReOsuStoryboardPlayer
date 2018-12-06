using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ReOsuStoryBoardPlayer
{
    public static class Log
    {
        private static long _currentTime = 0;

        private static bool _outPutWithColor = true;


        private static bool _ableLog = true;

        public static bool AbleLog
        {
            get => !Setting.MiniMode && _ableLog;
            set => _ableLog = value;
        }
        
        public static bool AbleDebugLog { get; set; } = false;

        public static bool IsColorOutput { get { return _outPutWithColor; } set { _outPutWithColor = value; } }

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
            long min = timePass / (60 * 1000), sec = (timePass - min * (60 * 1000)) / 1000, ms = timePass - min * 60000 - sec * 1000;

            return string.Format("{0:D2}:{1:D2}.{2:D3}", min, sec, ms);
        }

        private static string _buildLogMessage(string message, LogLevel level)
        {
            var stack = new StackTrace();
            int logMethodPosition = /*stack.FrameCount-*/6;
            var frame = stack.GetFrame(logMethodPosition);
            string methodName;

            if (frame != null)
                methodName = $"{frame.GetMethod().DeclaringType}::{frame.GetMethod().Name}";
            else
                methodName = "<Unknown Method>";

            string outPut = string.Format("[{0}]{2}.{1}():\n>>{3}\n", _getTimeStr(), methodName, level.ToString(), message);
            return outPut;
        }

        private static void _renderColor(ref string message, LogLevel level)
        {
            int index = (int)level;
            Console.ForegroundColor = colors[(index) * 2 + 0];
            Console.BackgroundColor = colors[(index) * 2 + 1];
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void _log(string message, LogLevel level)
        {
            if (!AbleLog)
                return;

            string output = _buildLogMessage(message, level);

            if (_outPutWithColor)
            {
                _renderColor(ref output, level);
                return;
            }

            Console.WriteLine(output);
        }

        public static void User(string message, params object[] paramsArr)
        {
            _log(paramsArr.Length != 0 ? string.Format(message, paramsArr) : message, LogLevel.User);
        }

        public static void Warn(string message, params object[] paramsArr)
        {
            _log(paramsArr.Length != 0 ? string.Format(message, paramsArr) : message, LogLevel.Warn);
        }

        public static void Error(string message, params object[] paramsArr)
        {
            _log(paramsArr.Length != 0 ? string.Format(message, paramsArr) : message, LogLevel.Error);
        }

        public static void Debug(string message, params object[] paramsArr)
        {
            if (!AbleDebugLog)
                return;
            _log(paramsArr.Length != 0 ? string.Format(message, paramsArr) : message, LogLevel.Debug);
        }

        public static void Write(string message, params object[] paramsArr)
        {
            _log(paramsArr.Length != 0 ? string.Format(message, paramsArr) : message, LogLevel.None);
        }
    }
}