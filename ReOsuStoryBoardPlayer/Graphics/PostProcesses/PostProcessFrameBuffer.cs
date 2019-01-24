using OpenTK.Graphics.OpenGL;
using System;

namespace ReOsuStoryboardPlayer.Graphics.PostProcesses
{
    public class PostProcessFrameBuffer : IDisposable
    {
        private int _fbo;
        public int ColorTexture { get; }

        private int _w, _h;

        public PostProcessFrameBuffer(int w, int h)
        {
            _w=w;
            _h=h;

            _fbo=GL.GenFramebuffer();
            ColorTexture=CraeteColorTexture();

            BuildFrameBuffer();
        }

        private void BuildFrameBuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorTexture, 0);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private int CraeteColorTexture()
        {
            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _w, _h, 0,
                    PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return handle;
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(_fbo);
            GL.DeleteTexture(ColorTexture);
        }
    }
}