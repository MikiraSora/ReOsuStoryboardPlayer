using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace ReOsuStoryBoardPlayer
{
    [Serializable]
    public class Texture :IDisposable
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

        Vector _textureSize;

        string name;

        public int Width { get { return (int)_textureSize.x; } }
        public int Height { get { return (int)_textureSize.y; } }

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

        public Texture(byte[] bytes)
        {
            name = "Texture";
            _id = 0;

            LoadFromByteArray(bytes);
        }

        public Texture(Bitmap bitmap)
        {
            name = "Texture";
            _id = 0;

            LoadFromBitmap(bitmap);
        }

        public Texture(int id, int width, int height)
        {
            name = "Texture";
            _id = 0;

            LoadFromTextureId(id, width, height);
        }

        public void LoadFromFile(string filename)
        {
            if (!String.IsNullOrEmpty(filename))
            {
                if (File.Exists(filename))
                {
                    _filename = filename;

                    GL.GenTextures(1, out _id);

                    GL.BindTexture(TextureTarget.Texture2D, _id);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                    Bitmap bmp = new Bitmap(filename);

                    _textureSize = new Vector(bmp.Width, bmp.Height);

                    BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                    bmp.UnlockBits(bmp_data);

                    bmp.Dispose();
                }
            }
        }

        public void LoadFromBitmap(Bitmap bmp)
        {
            _filename = filePath;

            GL.GenTextures(1, out _id);

            GL.BindTexture(TextureTarget.Texture2D, _id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            /*
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                _bytes = ms.ToArray();
                ms.Dispose();
            }
            */
            //bitmap = bmp;

            _textureSize = new Vector(bmp.Width, bmp.Height);

            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            bmp.UnlockBits(bmp_data);
        }

        public void LoadFromByteArray(byte[] fbytes)
        {
            if (fbytes != null)
            {
                //var _bytes = fbytes;
                GL.GenTextures(1, out _id);

                GL.BindTexture(TextureTarget.Texture2D, _id);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                MemoryStream ms = new MemoryStream();
                ms.Write(fbytes, 0, fbytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                Bitmap bmp = new Bitmap(ms);
                ms.Dispose();

                //bitmap = bmp;
                _textureSize = new Vector(bmp.Width, bmp.Height);

                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                bmp.UnlockBits(bmp_data);

                bmp.Dispose();
            }
        }

        public void LoadFromTextureId(int texGenId, int width, int height)
        {
            _id = texGenId;

            _textureSize = new Vector(width, height);
        }

        public void LoadFromData(IntPtr data, int width, int height)
        {
            if (data != null)
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

        ~Texture()
        {
            Dispose();
        }
    }
}
