using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.CompilerServices;
using ReOsuStoryBoardPlayer.Graphics;
using ReOsuStoryBoardPlayer.Kernel;

namespace ReOsuStoryBoardPlayer
{
    public class SpriteInstanceGroup : IDisposable
    {
        public uint Capacity { get; protected set; } = 0;

        private int _currentPostBaseIndex = 0;

        public static Matrix4 Projection => StoryboardWindow.ProjectionMatrix;
        public static Matrix4 View => StoryboardWindow.CameraViewMatrix;

        public int CurrentPostCount { get; private set; } = 0;

        private static byte[] PostData;
        private static int s_vbo_vertexBase, s_vbo_texPosBase;
        private const int BUFFER_COUNT = 3;
        private static int[] s_vaos = new int[BUFFER_COUNT]; 
        private static int[] s_vbos = new int[BUFFER_COUNT];
        private static BatchShader s_shader;

        private int _current_buffer_index = 0;

        private bool _window_resized = true;

        private SpriteInstanceGroup()
        {
            StoryboardWindow.CurrentWindow.Resize += (s, e) => _window_resized = true;
        }

        public string ImagePath { get; }

        public Texture Texture { get; }

        static SpriteInstanceGroup()
        {
            s_shader = new BatchShader();
            s_shader.Compile();

            PostData = new byte[_VertexSize * Setting.DrawCallInstanceCountMax];

            s_vbo_vertexBase = GL.GenBuffer();
            s_vbo_texPosBase = GL.GenBuffer();

            GL.GenVertexArrays(BUFFER_COUNT, s_vaos);
            GL.GenBuffers(BUFFER_COUNT, s_vbos);

            for (int i = 0; i < BUFFER_COUNT; i++)
            {
                _InitVertexBase(s_vaos[i]);
                _InitBuffer(s_vaos[i], s_vbos[i]);
            }

            StoryboardWindow.CurrentWindow.Closing += (s, e) =>
            {
                GL.DeleteBuffer(s_vbo_vertexBase);
                GL.DeleteBuffer(s_vbo_texPosBase);
                GL.DeleteBuffers(BUFFER_COUNT, s_vbos);
                GL.DeleteVertexArrays(BUFFER_COUNT, s_vaos);
            };
        }

        internal SpriteInstanceGroup(uint capacity, string image_path, Texture texture)
        {
            ImagePath = image_path;

            _bound.x = texture.Width;
            _bound.y = texture.Height;

            this.Capacity = capacity;

            this.Texture = texture;
        }

        private static int _VertexSize
        {
            get
            {
                /*-----------------CURRENT VERSION------------------ -
                        anchor(Hlaf)     flip(Hlaf)     color(byte)     modelMatrix(float)     
                        vec2(2)		     vec2(2)        vec4(4)         Matrix3x2(6)            
                */
                return (2 + 2) * 2/*Hlaf*/ + 4 * sizeof(byte) + 6 * sizeof(float);
            }
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

        private static void _InitVertexBase(int vao)
        {
            GL.BindVertexArray(vao);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, s_vbo_vertexBase);
                {
                    //绑定基本顶点
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * _cacheBaseVertex.Length),
                        _cacheBaseVertex, BufferUsageHint.StaticDraw);

                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * (2), 0);
                    GL.VertexAttribDivisor(1, 0);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, s_vbo_texPosBase);
                {
                    //绑定基本顶点
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * _cacheBaseTexPos.Length),
                        _cacheBaseTexPos, BufferUsageHint.StaticDraw);

                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * (2), 0);
                    GL.VertexAttribDivisor(0, 0);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.BindVertexArray(0);
        }

        private static void _InitBuffer(int vao,int vbo)
        {
            GL.BindVertexArray(vao);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                {
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(_VertexSize * Setting.DrawCallInstanceCountMax), IntPtr.Zero, BufferUsageHint.DynamicDraw);

                    //Anchor
                    GL.EnableVertexAttribArray(2);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.HalfFloat, false, _VertexSize, 0);
                    GL.VertexAttribDivisor(2, 1);

                    //Color
                    GL.EnableVertexAttribArray(3);
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, _VertexSize, 4);
                    GL.VertexAttribDivisor(3, 1);

                    //filp
                    GL.EnableVertexAttribArray(4);
                    GL.VertexAttribPointer(4, 2, VertexAttribPointerType.HalfFloat, false, _VertexSize, 8);
                    GL.VertexAttribDivisor(4, 1);

                    //ModelMatrix
                    GL.EnableVertexAttribArray(5);
                    GL.VertexAttribPointer(5, 2, VertexAttribPointerType.Float, false, _VertexSize, 12);
                    GL.VertexAttribDivisor(5, 1);
                    GL.EnableVertexAttribArray(6);
                    GL.VertexAttribPointer(6, 2, VertexAttribPointerType.Float, false, _VertexSize, 20);
                    GL.VertexAttribDivisor(6, 1);
                    GL.EnableVertexAttribArray(7);
                    GL.VertexAttribPointer(7, 2, VertexAttribPointerType.Float, false, _VertexSize, 28);
                    GL.VertexAttribDivisor(7, 1);
                    GL.EnableVertexAttribArray(8);
                    GL.VertexAttribPointer(8, 2, VertexAttribPointerType.Float, false, _VertexSize, 36);
                    GL.VertexAttribDivisor(8, 1);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.BindVertexArray(0);
        }

        private readonly Half HalfNegativeOne = new Half(-1f);
        private readonly Half HalfOne = new Half(1f);

        public void PostRenderCommand(Vector position, float z_other, Vector bound, float rotate, Vector scale, HalfVector anchor, ByteVec4 color, bool vertical_flip, bool horizon_flip)
        {
            /*-----------------CURRENT VERSION------------------ -
			*   anchor(Hlaf)	    color(byte)         modelMatrix
			*   vec2(2)		        vec4(4)             Matrix3x2(6)
			*/

            
            var is_xflip = Math.Sign(scale.x);
            var is_yflip = Math.Sign(scale.y);

            //adjust scale transform which value is negative
            horizon_flip=horizon_flip|(is_xflip<0);
            vertical_flip=vertical_flip|(is_yflip<0);
            float scalex = is_xflip*scale.x*bound.x;
            float scaley = is_yflip*scale.y*bound.y;

            //Create ModelMatrix
            float cosa = (float)Math.Cos(rotate);
            float sina = (float)Math.Sin(rotate);

            Matrix3x2 model = Matrix3x2.Zero;
            model.Row0.X = cosa * scalex;
            model.Row0.Y = -sina * scalex;
            model.Row1.X = sina * scaley;
            model.Row1.Y = cosa * scaley;

            model.Row2.X = position.x - StoryboardWindow.SB_WIDTH / 2f;
            model.Row2.Y = -position.y + StoryboardWindow.SB_HEIGHT / 2f;

            unsafe
            {
                //Anchor write
                fixed (byte* ptr = &PostData[_currentPostBaseIndex])
                {
                    //anchor
                    int* hpv = (int*)(ptr+0);
                    *hpv = *(int*)&anchor;

                    //color
                    int* ip = (int*)(ptr+4);
                    *ip = *(int*)&color;

                    //flip write
                    Half* hp = (Half*)(ptr+8);
                    hp[0] = horizon_flip ? HalfNegativeOne : HalfOne;
                    hp[1] = vertical_flip ? HalfNegativeOne : HalfOne;

                    Unsafe.CopyBlock(ptr+12, &model.Row0.X, 2 * 3 * sizeof(float));
                }
            }

            CurrentPostCount++;
            _currentPostBaseIndex += _VertexSize;
            if (CurrentPostCount >= Capacity)
            {
                FlushDraw();
            }
        }

        public void PostRenderCommand(Vector position, float z_orther, float rotate, Vector scale, HalfVector anchor, ByteVec4 color, bool vertical_flip, bool horizon_flip) => PostRenderCommand(position, z_orther, _bound, rotate, scale, anchor, color, vertical_flip, horizon_flip);

        private void Draw()
        {
            s_shader.Begin();

            if (_window_resized)
            {
                var VP = Projection * View;
                s_shader.PassUniform("ViewProjection", VP);
                _window_resized = false;
            }
            s_shader.PassUniform("diffuse", Texture);

            GL.BindBuffer(BufferTarget.ArrayBuffer, s_vbos[_current_buffer_index]);
            {
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(0), (IntPtr)(_VertexSize * CurrentPostCount), PostData);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(s_vaos[_current_buffer_index]);
            {
                GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, CurrentPostCount);
            }
            GL.BindVertexArray(0);
            _current_buffer_index = (_current_buffer_index + 1) % s_vbos.Length;
        }

        public void FlushDraw()
        {
            Draw();
            Clear();
        }

        private void Clear()
        {
            CurrentPostCount = 0;
            _currentPostBaseIndex = 0;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    Texture.Dispose();

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