using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleRenderFramework;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;

namespace Test
{
	public class TestGameWindow : Window
	{
        public ReOsuStoryBoardPlayer.StoryBoardInstance instance;

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            #region InitGraphics

            GL.DepthRange(0, 100);

            #endregion

            instance = new ReOsuStoryBoardPlayer.StoryBoardInstance(@"H:\SBTest\413707 Milkychan - Stronger Than You -Chara Response-");

            instance.Start();

            Title = "Esu!StoryBoardPlayer";

            VSync = VSyncMode.Off;

            Engine.LoadSystem(new ReOsuStoryBoardPlayer.StoryboardRenderSystem(instance));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);          

            instance.Update((float)e.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
        }
    }
}
