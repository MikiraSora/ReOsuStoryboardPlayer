using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.StatusViewer
{
    public class StatusViewerDebugger : DebuggerBase
    {
        public override void Init()
        {
            Priority=UpdatePriority.PerSecond;
        }

        public override void Term()
        {

        }

        public override void Update()
        {
            Log.Debug($"{MusicPlayerManager.ActivityPlayer.CurrentTime} PickCommand CacheHit:{CommandTimeline.CacheHit} ({(CommandTimeline.CacheHit*1.0f/(CommandTimeline.CacheHit+CommandTimeline.RecalcHit)*100).ToString("F2")}%) RecalcHit:{CommandTimeline.RecalcHit}");
            CommandTimeline.CacheHit=CommandTimeline.RecalcHit=0;
        }
    }
}
