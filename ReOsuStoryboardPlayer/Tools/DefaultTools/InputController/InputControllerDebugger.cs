﻿using OpenTK.Input;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ReOsuStoryboardPlayer.Player;
using System;

namespace ReOsuStoryboardPlayer.Tools.DefaultTools.InputController
{
    internal class InputControllerDebugger : ToolBase
    {
        public override void Init()
        {
            ToolManager.KeyboardPress+=DebuggerManager_KeyBoardPress;
            ToolManager.MouseWheel += ToolManager_MouseWheel;
        }

        private void ToolManager_MouseWheel(MouseWheelEventArgs e)
        {
            var time = -e.OffsetY * 125;

            if (MusicPlayerManager.ActivityPlayer is MusicPlayer player)
                player.Jump(player.CurrentTime + time, true);
        }

        private void DebuggerManager_KeyBoardPress(Keys e)
        {
            switch (e)
            {
                case Keys.F:
                    StoryboardWindow.CurrentWindow.SwitchFullscreen();
                    break;

                case Keys.B:
                    StoryboardWindow.CurrentWindow.ApplyBorderless(!StoryboardWindow.CurrentWindow.IsBorderless);
                    break;

                case Keys.Escape:
                    MainProgram.Exit();
                    break;

                case Keys.Left:
                    FastJump(-2000);
                    break;

                case Keys.Right:
                    FastJump(2000);
                    break;

                case Keys.Space:
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
            player.Jump(Math.Min(player.Length, Math.Max(player.CurrentTime+offset, 0)), false);
        }

        public override void Term()
        {
            ToolManager.KeyboardPress-=DebuggerManager_KeyBoardPress;
        }

        public override void Update()
        {
        }
    }
}