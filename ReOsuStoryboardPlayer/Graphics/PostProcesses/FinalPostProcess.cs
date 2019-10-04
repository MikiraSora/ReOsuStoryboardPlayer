using OpenTK.Graphics.OpenGL;
using ReOsuStoryboardPlayer.Graphics.PostProcesses.Shaders;
using ReOsuStoryBoardPlayer.Graphics;

namespace ReOsuStoryboardPlayer.Graphics.PostProcesses
{
    internal class FinalPostProcess : APostProcess
    {
        private FinalShader _shader = new FinalShader();

        public FinalPostProcess()
        {
            _shader.Compile();
        }

        protected override void OnUseShader()
        {
            int tex = PrevFrameBuffer?.ColorTexture??0;
            GL.BindTexture(TextureTarget.Texture2D, tex);
            _shader.Begin();
        }

        protected override void OnPreRender()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, RenderKernel.DefaultFrameBuffer);
            GL.Viewport(0, 0, RenderKernel.Width, RenderKernel.Height);
        }

        protected override void OnPostRender()
        {
        }
    }
}