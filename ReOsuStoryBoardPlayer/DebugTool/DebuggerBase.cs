using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
