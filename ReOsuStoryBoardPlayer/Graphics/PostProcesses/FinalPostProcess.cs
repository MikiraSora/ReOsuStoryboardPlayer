using OpenTK.Graphics.OpenGL;
using ReOsuStoryboardPlayer.Graphics.PostProcesses.Shaders;

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
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, StoryboardWindow.CurrentWindow.Width, StoryboardWindow.CurrentWindow.Height);
        }

        protected override void OnPostRender()
        {
        }
    }
}