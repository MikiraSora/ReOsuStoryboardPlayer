using OpenTK.Input;
using ReOsuStoryBoardPlayer.Player;
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

                case Key.Left:
                    FastJump(-2000);
                    break;

                case Key.Right:
                    FastJump(2000);
                    break;

                case Key.Space:
                    if (MusicPlayerManager.ActivityPlayer.IsPlaying)
                        MusicPlayerManager.ActivityPlayer.Pause();
                    else
                        MusicPlayerManager.ActivityPlayer.Play();
                    break;
            }
        }

        private void FastJump(int offset)
        {
            var player = MusicPlayerManager.ActivityPlayer;
            player.Jump(Math.Min(player.Length, Math.Max(player.CurrentTime+offset,0)),false);
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
