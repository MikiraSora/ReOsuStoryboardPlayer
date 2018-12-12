using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.CompilerServices;

namespace ReOsuStoryBoardPlayer
{
    public class SpriteInstanceGroup : IDisposable
    {
        public uint Capacity { get; protected set; } = 0;

        private int _currentPostCount = 0;

        private static BatchShader _shader;

        public static Matrix4 Projection { get { return StoryboardWindow.ProjectionMatrix; } }
        public static Matrix4 View { get { return StoryboardWindow.CameraViewMatrix; } }

        public int CurrentPostCount { get => _currentPostCount; }

        private int _vao, _vbo, _vbo_vertexBase, _vbo_texPosBase;

        private SpriteInstanceGroup()
        {

        }

        private Texture texture;

        public string ImagePath { get; private set; }

        public Texture Texture { get => texture; }

        static SpriteInstanceGroup()
        {
            _shader = new BatchShader();
            _shader.Compile();
        }

        internal SpriteInstanceGroup(uint capacity, string image_path, Texture texture)
        {
            ImagePath = image_path;

            _bound.x = texture.Width;
            _bound.y = texture.Height;

            this.Capacity = capacity;

            this.texture = texture;

            _buildBuffer();

            PostData = new float[_calculateCapacitySize() * capacity];
        }

        ~SpriteInstanceGroup()
        {
            _deleteBuffer();
        }

        private int _calculateCapacitySize()
        {
            /*-----------------CURRENT VERSION------------------ -
					*orther		anchor(Hlaf)    color(byte)     modelMatrix(float)      flip(Hlaf)
					*float(1)	vec2(2)		    vec4(4)         Matrix3x2(6)            vec2(2)
			*/
            return (1 + 6) * sizeof(float) + (2 + 2) * 2/*Hlaf*/ + 4 * sizeof(byte) ;
        }

        private Vector _bound;

        private static float[] _cacheBaseVertex = new float[] {
                -0.5f, 0.5f,
                 0.5f, 0.5f,
                 0.5f, -0.5f,
                -0.5f, -0.5f,
        };

        private static float[] _cacheBaseTexPos = new float[] {
                 0,0,
                 1,0,
                 1,1,
                 0,1
        };

        private void _buildBuffer()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _vbo_vertexBase = GL.GenBuffer();
            _vbo_texPosBase = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_vertexBase);
                {
                    //绑定基本顶点
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * _cacheBaseVertex.Length), _cacheBaseVertex, BufferUsageHint.StreamDraw);

                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * (2), 0);
                    GL.VertexAttribDivisor(1, 0);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_texPosBase);
                {
                    //绑定基本顶点
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * _cacheBaseTexPos.Length), _cacheBaseTexPos, BufferUsageHint.StreamDraw);

                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * (2), 0);
                    GL.VertexAttribDivisor(0, 0);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                {
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(_calculateCapacitySize() * Capacity), IntPtr.Zero, BufferUsageHint.DynamicDraw);

                    //orther
                    GL.EnableVertexAttribArray(2);
                    GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 0);//THIS: 最后一个参数是字节数
                    GL.VertexAttribDivisor(2, 1);

                    //Anchor
                    GL.EnableVertexAttribArray(3);
                    GL.VertexAttribPointer(3, 2, VertexAttribPointerType.HalfFloat, false, _calculateCapacitySize(), 4);
                    GL.VertexAttribDivisor(3, 1);

                    //Color
                    GL.EnableVertexAttribArray(4);
                    GL.VertexAttribPointer(4, 4, VertexAttribPointerType.UnsignedByte, true, _calculateCapacitySize(), 8);
                    GL.VertexAttribDivisor(4, 1);

                    //filp
                    GL.EnableVertexAttribArray(5);
                    GL.VertexAttribPointer(5, 2, VertexAttribPointerType.HalfFloat, false, _calculateCapacitySize(), 12);
                    GL.VertexAttribDivisor(5, 1);

                    //ModelMatrix
                    GL.EnableVertexAttribArray(6);
                    GL.VertexAttribPointer(6, 2, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 16);
                    GL.VertexAttribDivisor(6, 1);
                    GL.EnableVertexAttribArray(7);
                    GL.VertexAttribPointer(7, 2, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 24);
                    GL.VertexAttribDivisor(7, 1);
                    GL.EnableVertexAttribArray(8);
                    GL.VertexAttribPointer(8, 2, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 32);
                    GL.VertexAttribDivisor(8, 1);
                    GL.EnableVertexAttribArray(9);
                    GL.VertexAttribPointer(9, 2, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 40);
                    GL.VertexAttribDivisor(9, 1);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.BindVertexArray(0);
        }

        private void _deleteBuffer()
        {
            //GL.DeleteBuffer(_vbo);
            //GL.DeleteVertexArray(_vao);
        }

        private Vector3 _staticCacheAxis = new Vector3(0, 0, -1);

        private float[] PostData;

        public void PostRenderCommand(Vector position, float z_other, Vector bound, float rotate, Vector scale, Vector anchor, Vec4 color, bool vertical_flip, bool horizon_flip)
        {
            /*-----------------CURRENT VERSION------------------ -
			*orther		anchor(Hlaf)	    color(byte)         modelMatrix
			*float(1)	vec2(2)		        vec4(4)             Matrix3x2(6)
			*/

            int base_index = _currentPostCount * _calculateCapacitySize() / sizeof(float);

            //Create ModelMatrix
            float cosa = (float)Math.Cos(rotate);
            float sina = (float)Math.Sin(rotate);
            float scalex = scale.x * bound.x;
            float scaley = scale.y * bound.y;

            Matrix3x2 model = Matrix3x2.Zero;
            model.Row0.X = cosa * scalex;
            model.Row0.Y = -sina * scalex;
            model.Row1.X = sina * scaley;
            model.Row1.Y = cosa * scaley;

            model.Row2.X = position.x - StoryboardWindow.SB_WIDTH / 2f;
            model.Row2.Y = -position.y + StoryboardWindow.SB_HEIGHT / 2f;

            //Z float
            PostData[base_index + 0] = 0;

            unsafe
            {
                //Anchor write
                fixed (float* ptr = &PostData[base_index + 1])
                {
                    Half* p = (Half*)ptr;
                    p[0] = (Half)anchor.x;
                    p[1] = (Half)anchor.y;
                }

                //Color write
                fixed (float* ptr = &PostData[base_index + 2])
                {
                    byte* p = (byte*) ptr;
                    p[0] = (byte)(color.x * 255f);
                    p[1] = (byte)(color.y * 255f);
                    p[2] = (byte)(color.z * 255f);
                    p[3] = (byte)(color.w * 255f);
                }

                //flip write
                fixed (float* ptr = &PostData[base_index + 3])
                {
                    Half* p = (Half*) ptr;
                    p[0] =(Half)(horizon_flip ? -1 : 1);
                    p[1] =(Half)(vertical_flip ? -1 : 1);
                }

                //ModelMatrix Write 
                unsafe
                {
                    fixed (void* ptr = &PostData[base_index + 4])
                    {
                        Unsafe.CopyBlock(ptr, &model.Row0.X, 2 * 3 * sizeof(float));
                    }
                }
            }

            _currentPostCount++;
            if (_currentPostCount >= Capacity)
            {
                FlushDraw();
            }
        }

        public void PostRenderCommand(Vector position, float z_orther, float rotate, Vector scale, Vector anchor, Vec4 color, bool vertical_flip, bool horizon_flip) => PostRenderCommand(position, z_orther, _bound, rotate, scale, anchor, color, vertical_flip, horizon_flip);

        private void Draw()
        {
            _shader.Begin();
            var VP = Projection * (View);

            _shader.PassUniform("diffuse", texture);
            _shader.PassUniform("ViewProjection", VP);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            {
                GL.BufferSubData<float>(BufferTarget.ArrayBuffer, (IntPtr)(0), (IntPtr)(_calculateCapacitySize() * CurrentPostCount), PostData);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(_vao);
            {
                GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, _currentPostCount);
            }

            GL.BindVertexArray(0);

            //_shader.Clear();
            _shader.End();
        }

        public void FlushDraw()
        {
            Draw();
            Clear();
        }

        private void Clear()
        {
            _currentPostCount = 0;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    texture.Dispose();

                GL.DeleteBuffer(_vbo);
                GL.DeleteVertexArray(_vao);

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}