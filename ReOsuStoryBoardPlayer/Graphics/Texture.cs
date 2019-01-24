using OpenTK.Graphics.OpenGL;
using ReOsuStoryBoardPlayer.Core.PrimitiveValue;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ReOsuStoryBoardPlayer
{
    [Serializable]
    public class Texture : IDisposable
    {
        protected int _id;

        //[NonSerialized]
        //Bitmap _bitmap;

        [NonSerialized]
        private string _filename;

        /*
        [Browsable(false)]
        public Bitmap bitmap { get { return _bitmap; } internal set { _bitmap = value; } }
        */

        [Browsable(false)]
        public int ID { get { return _id; } }

        public string filePath { get { return _filename; } set { LoadFromFile(value); } }

        private Vector _textureSize;

        private string name;

        public int Width { get { return (int)_textureSize.X; } }
        public int Height { get { return (int)_textureSize.Y; } }

        public Texture()
        {
            name = "Texture";
            _id = 0;
        }

        public Texture(string fname)
        {
            name = "Texture";
            _id = 0;

            LoadFromFile(fname);
        }

        public void LoadFromFile(string filename)
        {
            if (!String.IsNullOrEmpty(filename))
            {
                _filename=filename;

                GL.GenTextures(1, out _id);

                GL.BindTexture(TextureTarget.Texture2D, _id);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                Bitmap bmp = new Bitmap(filename);

                _textureSize=new Vector(bmp.Width, bmp.Height);

                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                bmp.UnlockBits(bmp_data);

                bmp.Dispose();
            }
        }

        public void LoadFromData(IntPtr data, int width, int height)
        {
            if (data != IntPtr.Zero)
            {
                GL.GenTextures(1, out _id);

                GL.BindTexture(TextureTarget.Texture2D, _id);

                _textureSize = new Vector(width, height);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data);
            }
        }

        public override string ToString()
        {
            return name;
        }

        public void Dispose()
        {
            GL.DeleteTexture(_id);
        }
    }
}