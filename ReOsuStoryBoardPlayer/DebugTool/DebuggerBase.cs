namespace ReOsuStoryBoardPlayer.DebugTool
{
    public abstract class DebuggerBase
    {
        public UpdatePriority Priority { get; protected set; }

        public abstract void Init();

        public abstract void Term();

        public abstract void Update();

        public void UninstallSelf() => DebuggerManager.RemoveDebugger(this);

        public void InstallSelf() => DebuggerManager.AddDebugger(this);
    }
}