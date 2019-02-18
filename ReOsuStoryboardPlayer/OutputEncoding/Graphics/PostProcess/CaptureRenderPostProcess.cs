using OpenTK.Graphics.OpenGL;
using ReOsuStoryboardPlayer;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Graphics.PostProcesses;
using ReOsuStoryboardPlayer.Graphics.PostProcesses.Shaders;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.OutputEncoding.Kernel;
using ReOsuStoryboardPlayer.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace ReOsuStoryBoardPlayer.OutputEncoding.Graphics.PostProcess
{
    public class CaptureRenderPostProcess : APostProcess
    {
        PostProcessFrameBuffer frame_buffer = new PostProcessFrameBuffer(PlayerSetting.Width, PlayerSetting.Height);

        public byte[] RenderPixels { get; } = new byte[PlayerSetting.Width*3*PlayerSetting.Height];

        int store_prev_fbo = 0;
        int[] store_prev_viewport = new int[4];

        private Shader _output_shader = new VectialFlipShader();
        private Shader _final_shader = new FinalShader();

        private EncodingKernel encoding_kernel;
        public EncodingKernel Kernal => encoding_kernel??(encoding_kernel=ToolManager.GetTool<EncodingKernel>());

        public CaptureRenderPostProcess()
        {
            _output_shader.Compile();
            _final_shader.Compile();
        }

        protected override void OnUseShader()
        {
            int tex = PrevFrameBuffer?.ColorTexture??0;
            GL.BindTexture(TextureTarget.Texture2D, tex);
        }

        protected override void OnRender()
        {
            _output_shader.Begin();
            //backup
            GL.GetInteger(GetPName.Viewport, store_prev_viewport);
            store_prev_fbo=GL.GetInteger(GetPName.FramebufferBinding);

            frame_buffer.Bind();
            GL.Viewport(0, 0, StoryboardWindow.CurrentWindow.Width, StoryboardWindow.CurrentWindow.Height);

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            _final_shader.Begin();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, store_prev_fbo);
            GL.Viewport(store_prev_viewport[0], store_prev_viewport[1], store_prev_viewport[2], store_prev_viewport[3]);
            
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            //output
            GL.BindTexture(TextureTarget.Texture2D, frame_buffer.ColorTexture);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgr, PixelType.UnsignedByte, RenderPixels);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void OnResize()
        {
            base.OnResize();


        }
    }
}
