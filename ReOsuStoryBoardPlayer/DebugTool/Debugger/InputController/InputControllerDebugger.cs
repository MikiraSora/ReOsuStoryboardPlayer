using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.InputController
{
    class InputControllerDebugger : DebuggerBase
    {
        public override void Init()
        {
            DebuggerManager.KeyboardPress+=DebuggerManager_KeyBoardPress;
        }

        private void DebuggerManager_KeyBoardPress(Key e)
        {
            switch (e)
            {
                case Key.F:
                    StoryboardWindow.CurrentWindow.SwitchFullscreen();
                    break;

                case Key.B:
                    StoryboardWindow.CurrentWindow.ApplyBorderless(!StoryboardWindow.CurrentWindow.IsBorderless);
                    break;

                case Key.Escape:
                    MainProgram.Exit();
                    break;
            }
        }

        public override void Term()
        {
            DebuggerManager.KeyboardPress-=DebuggerManager_KeyBoardPress;
        }

        public override void Update()
        {

        }
    }
}
