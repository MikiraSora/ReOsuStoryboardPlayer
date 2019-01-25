namespace ReOsuStoryboardPlayer.Tools.DefaultTools.ObjectsSequenceViewer
{
    public class ObjectsSequenceViewerDebugger : ToolBase
    {
        private ObjectsSequenceViewer window;

        public override void Init()
        {
            window=new ObjectsSequenceViewer();
            window.Show();
        }

        public override void Term()
        {
            window.Close();
        }

        public override void Update()
        {
        }
    }
}