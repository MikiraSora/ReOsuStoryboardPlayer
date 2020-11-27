using OpenTK.Input;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ReOsuStoryboardPlayer.Tools
{
    public static class ToolManager
    {
        private static Timer timer;

        private static HashSet<ToolBase> register_tools = new HashSet<ToolBase>();

        static ToolManager()
        {
            timer=new Timer(SecondUpdate, null, 0, 1000);
#if DEBUG

#endif
        }

        public static void AddTool(ToolBase tool)
        {
            if (register_tools.Contains(tool))
                return;

            tool.Init();
            register_tools.Add(tool);
            Log.Debug($"Add tool:{tool.GetType().Name}");
        }

        public static void RemoveTool(ToolBase tool)
        {
            if (!register_tools.Contains(tool))
                return;

            tool.Term();
            register_tools.Remove(tool);
        }

        public static T GetTool<T>() where T : ToolBase => register_tools.OfType<T>().FirstOrDefault();

        public static T GetOrCreateTool<T>() where T : ToolBase, new()
        {
            var tool = GetTool<T>();

            if (tool==null)
            {
                tool=new T();
                AddTool(tool);
            }

            return tool;
        }

        private static void SecondUpdate(object state)
        {
            foreach (var tool in register_tools.Where(d => d.Priority==UpdatePriority.PerSecond))
                tool.Update();
        }

        public static void FrameUpdate()
        {
            foreach (var tool in register_tools.Where(d => d.Priority==UpdatePriority.EveryFrame))
                tool.Update();
        }

        public static void Close()
        {
            timer.Dispose();

            foreach (var tool in register_tools)
            {
                tool.Term();
            }

            register_tools.Clear();
        }

        #region Event

        public static event Action<float, float, MouseInput> MouseClick;

        public static event Action<float, float> MouseMove;

        public static event Action<Keys> KeyboardPress;

        public static event Action<MouseWheelEventArgs> MouseWheel;

        public static event Action BeforeRender;

        public static event Action AfterRender;

        internal static void TrigClick(float x, float y, MouseInput input) => MouseClick?.Invoke(x, y, input);

        internal static void TrigMove(float x, float y) => MouseMove?.Invoke(x, y);

        internal static void TrigMouseWheel(MouseWheelEventArgs e) => MouseWheel?.Invoke(e);

        internal static void TrigBeforeRender() => BeforeRender?.Invoke();

        internal static void TrigAfterRender() => AfterRender?.Invoke();

        internal static void TrigKeyPress(Keys key) => KeyboardPress?.Invoke(key);

        #endregion Event
    }
}