namespace ReOsuStoryboardPlayer.Tools
{
    public abstract class ToolBase
    {
        public UpdatePriority Priority { get; protected set; }

        public abstract void Init();

        public abstract void Term();

        public abstract void Update();

        public void UninstallSelf() => ToolManager.RemoveTool(this);

        public void InstallSelf() => ToolManager.AddTool(this);
    }
}