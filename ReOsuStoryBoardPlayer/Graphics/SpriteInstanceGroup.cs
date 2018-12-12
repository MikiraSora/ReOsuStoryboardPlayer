﻿using OpenTK;
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

        private int _currentPostCount = 0;

        private static BatchShader _shader;

        public static Matrix4 Projection { get { return StoryboardWindow.ProjectionMatrix; } }
        public static Matrix4 View { get { return StoryboardWindow.CameraViewMatrix; } }

        public int CurrentPostCount { get => _currentPostCount; }

        private int _vbo_vertexBase, _vbo_texPosBase;
        private int[] _vaos = new int[3]; 
        private int[] _vbos = new int[3];

        private int _current_buffer_index = 0;

        private bool _window_resized = true;

        private SpriteInstanceGroup()
        {
            StoryboardWindow.CurrentWindow.Resize += (s, e) => _window_resized = true;
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

            _vbo_vertexBase = GL.GenBuffer();
            _vbo_texPosBase = GL.GenBuffer();

            GL.GenVertexArrays(3,_vaos);
            GL.GenBuffers(3,_vbos);

            for (int i = 0; i < 3; i++)
            {
                _InitVertexBase(_vaos[i]);
                _InitBuffer(_vaos[i],_vbos[i]);
            }

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

        private void _InitVertexBase(int vao)
        {
            GL.BindVertexArray(vao);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_vertexBase);
                {
                    //绑定基本顶点
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * _cacheBaseVertex.Length),
                        _cacheBaseVertex, BufferUsageHint.StaticDraw);

                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * (2), 0);
                    GL.VertexAttribDivisor(1, 0);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_texPosBase);
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

        private void _InitBuffer(int vao,int vbo)
        {
            GL.BindVertexArray(vao);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
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

        private float[] PostData;
        private readonly Half HalfNegativeOne = new Half(-1f);
        private readonly Half HalfOne = new Half(1f);

        public void PostRenderCommand(Vector position, float z_other, Vector bound, float rotate, Vector scale, HalfVector anchor, Vec4 color, bool vertical_flip, bool horizon_flip)
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
            PostData[base_index + 0] = z_other;

            unsafe
            {
                //Anchor write
                fixed (float* ptr = &PostData[base_index + 1])
                {
                    Half* p = (Half*)ptr;
                    p[0] = anchor.x;
                    p[1] = anchor.y;
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
                    p[0] = horizon_flip ? HalfNegativeOne : HalfOne;
                    p[1] = vertical_flip ? HalfNegativeOne : HalfOne;
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

        public void PostRenderCommand(Vector position, float z_orther, float rotate, Vector scale, HalfVector anchor, Vec4 color, bool vertical_flip, bool horizon_flip) => PostRenderCommand(position, z_orther, _bound, rotate, scale, anchor, color, vertical_flip, horizon_flip);

        private void Draw()
        {
            _shader.Begin();

            if (_window_resized)
            {
                var VP = Projection * (View);
                _shader.PassUniform("ViewProjection", VP);
                _window_resized = false;
            }
            _shader.PassUniform("diffuse", texture);
            _shader.PassUniform("MaxZ",(float)StoryboardWindow.CurrentWindow.StoryBoardInstance.MaxZ);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbos[_current_buffer_index]);
            {
                GL.BufferSubData<float>(BufferTarget.ArrayBuffer, (IntPtr)(0), (IntPtr)(_calculateCapacitySize() * CurrentPostCount), PostData);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(_vaos[_current_buffer_index]);
            {
                GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, _currentPostCount);
            }
            GL.BindVertexArray(0);
            _current_buffer_index = (_current_buffer_index + 1) % _vbos.Length;
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

                GL.DeleteBuffer(_vbo_vertexBase);
                GL.DeleteBuffer(_vbo_texPosBase);
                GL.DeleteBuffers(3,_vbos);
                GL.DeleteVertexArrays(3,_vaos);

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