using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using ReOsuStoryBoardPlayer.Graphics.PostProcesses.Shaders;

namespace ReOsuStoryBoardPlayer.Graphics.PostProcesses
{
    class ClipPostProcess:APostProcess
    {
        ClipShader _shader = new ClipShader();

        public ClipPostProcess()
        {
            _shader.Compile();
        }

        protected override void OnUseShader()
        {
            int tex = LastFrameBuffer?.ColorTexture ?? 0;
            GL.BindTexture(TextureTarget.Texture2D, tex);
            _shader.Begin();
            _shader.PassUniform("view_width",StoryboardWindow.CurrentWindow.ViewWidth);
        }
    }
}
