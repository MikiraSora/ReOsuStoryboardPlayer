using OpenTK.Graphics.OpenGL;
using ReOsuStoryBoardPlayer.Graphics.PostProcesses.Shaders;

namespace ReOsuStoryBoardPlayer.Graphics.PostProcesses
{
    internal class ClipPostProcess : APostProcess
    {
        private ClipShader _shader = new ClipShader();

        public ClipPostProcess()
        {
            _shader.Compile();
        }

        protected override void OnUseShader()
        {
            int tex = PrevFrameBuffer?.ColorTexture??0;
            GL.BindTexture(TextureTarget.Texture2D, tex);
            _shader.Begin();
            _shader.PassUniform("view_width", StoryboardWindow.CurrentWindow.ViewWidth);
        }
    }
}