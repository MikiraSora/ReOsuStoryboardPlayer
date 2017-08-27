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

            instance = new ReOsuStoryBoardPlayer.StoryBoardInstance(@"H:\SBTest\511637 solfa featChata - I will");

            instance.Start();

            Title = "Esu!StoryBoardPlayer";

            VSync = VSyncMode.Off;

            Engine.LoadSystem(new ReOsuStoryBoardPlayer.StoryboardRenderSystem(instance));

            Schedule.addMainThreadUpdateTask(new Schedule.ScheduleTask(3, true, null, -1, (task,obj) => {
                instance.Update(3);
            }));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
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
