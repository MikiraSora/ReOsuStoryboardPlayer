using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectsSequenceViewer
{
    public class ObjectsSequenceViewerDebugger : DebuggerBase
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
