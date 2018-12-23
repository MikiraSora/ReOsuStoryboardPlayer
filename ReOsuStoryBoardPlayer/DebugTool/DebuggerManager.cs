using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool
{
    public static class DebuggerManager
    {
        private static Timer timer;

        private static HashSet<DebuggerBase> register_debuggers = new HashSet<DebuggerBase>();

        static DebuggerManager()
        {
            timer=new Timer(SecondUpdate, null, 0, 1000);
#if DEBUG

#endif
        }

        public static void AddDebugger(DebuggerBase debugger)
        {
            if (register_debuggers.Contains(debugger))
                return;

            debugger.Init();
            register_debuggers.Add(debugger);
            Log.Debug($"Add debugger:{debugger.GetType().Name}");
        }

        public static void RemoveDebugger(DebuggerBase debugger)
        {
            if (!register_debuggers.Contains(debugger))
                return;

            debugger.Term();
            register_debuggers.Remove(debugger);
        }

        public static T GetDebugger<T>() where T : DebuggerBase => register_debuggers.OfType<T>().FirstOrDefault();

        public static T GetOrCreateDebugger<T>() where T : DebuggerBase, new()
        {
            var debugger = GetDebugger
<T>();

            if (debugger==null)
            {
                debugger=new T();
                AddDebugger(debugger);
            }

            return debugger;
        }

        private static void SecondUpdate(object state)
        {
            foreach (var debugger in register_debuggers.Where(d => d.Priority==UpdatePriority.PerSecond))
                debugger.Update();
        }

        public static void FrameUpdate()
        {
            foreach (var debugger in register_debuggers.Where(d => d.Priority==UpdatePriority.EveryFrame))
                debugger.Update();
        }

        public static void Close()
        {
            timer.Dispose();

            foreach (var debugger in register_debuggers)
            {
                debugger.Term();
            }

            register_debuggers.Clear();
        }

        #region Event

        public static event Action<int,int,MouseInput> MouseClick;
        public static event Action<int, int> MouseMove;
        public static event Action<Key> KeyboardPress;
        public static event Action BeforeRender;
        public static event Action AfterRender;

        internal static void TrigClick(int x, int y, MouseInput input) => MouseClick?.Invoke(x, y, input);
        internal static void TrigMove(int x, int y) => MouseMove?.Invoke(x, y);
        internal static void TrigBeforeRender() => BeforeRender?.Invoke();
        internal static void TrigAfterRender() => AfterRender?.Invoke();
        internal static void TrigKeyPress(Key key) => KeyboardPress?.Invoke(key);

        #endregion
    }
}
