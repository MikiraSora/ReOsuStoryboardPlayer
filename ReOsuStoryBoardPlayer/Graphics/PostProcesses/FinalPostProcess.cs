using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using ReOsuStoryBoardPlayer.Graphics.PostProcesses.Shaders;

namespace ReOsuStoryBoardPlayer.Graphics.PostProcesses
{
    class FinalPostProcess:APostProcess
    {
        FinalShader _shader = new FinalShader();
        private int _tmp_fbo = 0;

        public FinalPostProcess()
        {
            _shader.Compile();
        }

        protected override void OnUseShader()
        {
            int tex = LastFrameBuffer?.ColorTexture ?? 0;
            GL.BindTexture(TextureTarget.Texture2D, tex);
            _shader.Begin();
        }

        protected override void OnPreRender()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);
        }

        protected override void OnPostRender()
        {
            
        }
    }
}
