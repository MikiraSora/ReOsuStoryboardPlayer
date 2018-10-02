using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using OpenTK;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;

namespace ReOsuStoryBoardPlayer
{
    public class SpriteInstanceGroup
    {
        public uint Capacity { get; protected set; } = 0;

        int _currentPostCount = 0;
        
        static BatchShader _shader;

        public static Matrix4 Projection { get { return StoryboardWindow.ProjectionMatrix; } }
        public static Matrix4 View { get { return StoryboardWindow.CameraViewMatrix; } }

        public int CurrentPostCount { get => _currentPostCount; }

        int _vao, _vbo, _vbo_vertexBase, _vbo_texPosBase;

        private SpriteInstanceGroup() { }

        Texture texture;

        public string ImagePath { get; private set; }

        public Texture Texture { get => texture; }

        static SpriteInstanceGroup()
        {
            _shader = new BatchShader();
            _shader.Compile();
        }

        internal SpriteInstanceGroup(uint capacity,string image_path, Texture texture)
        {
            ImagePath = image_path;

            _bound.x = texture.Width;
            _bound.y = texture.Height;

            this.Capacity = capacity;
            
            this.texture = texture;

            _buildBuffer();

            PostData = new float[_calculateCapacitySize()*capacity];
        }

        ~SpriteInstanceGroup()
        {
            _deleteBuffer();
        }

        int _calculateCapacitySize()
        {
            /*-----------------CURRENT VERSION------------------ -
					*orther		anchor	    color		bound       modelMatrix   flip
					*float(1)	vec2(2)		vec4(4)     vec2(2)     Matrix4(16)   vec2(2)
					*/
            return (1 + 2 + 4 + 2 + 16+2) * sizeof(float);
        }

        Vector _bound;

        static float[] _cacheBaseVertex = new float[] {
                0,0,
                0,-1,
                1,-1,
                1,0,
        };

        static float[] _cacheBaseTexPos = new float[] {
                 0,1,
                 0,0,
                 1,0,
                 1,1
        };

        void _buildBuffer()
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
                    GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 4);
                    GL.VertexAttribDivisor(3, 1);

                    //Color
                    GL.EnableVertexAttribArray(4);
                    GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 12);
                    GL.VertexAttribDivisor(4, 1);

                    //Bound
                    GL.EnableVertexAttribArray(5);
                    GL.VertexAttribPointer(5, 2, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 28);
                    GL.VertexAttribDivisor(5, 1);

                    //filp
                    GL.EnableVertexAttribArray(6);
                    GL.VertexAttribPointer(6, 2, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 36);
                    GL.VertexAttribDivisor(6, 1);

                    //ModelMatrix
                    GL.EnableVertexAttribArray(7);
                    GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 44);
                    GL.VertexAttribDivisor(7, 1);
                    GL.EnableVertexAttribArray(8);
                    GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 60);
                    GL.VertexAttribDivisor(8, 1);
                    GL.EnableVertexAttribArray(9);
                    GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 76);
                    GL.VertexAttribDivisor(9, 1);
                    GL.EnableVertexAttribArray(10);
                    GL.VertexAttribPointer(10, 4, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 92);
                    GL.VertexAttribDivisor(10, 1);

                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.BindVertexArray(0);
        }

        void _deleteBuffer()
        {
            //GL.DeleteBuffer(_vbo);
            //GL.DeleteVertexArray(_vao);
        }

        Vector3 _staticCacheAxis = new Vector3(0, 0, 1);

        float[] PostData;

        public void PostRenderCommand(Vector position, float z_other, Vector bound, float rotate, Vector scale, Vector anchor, Vec4 color,bool vertical_flip,bool horizon_flip)
        {
            /*-----------------CURRENT VERSION------------------ -
			*orther		anchor	    color		bound       modelMatrix
			*float(1)	vec2(2)		vec4(4)     vec2(2)     Matrix4(16)
			*/

            int base_index = _currentPostCount * _calculateCapacitySize()/sizeof(float);

            //Z float
            PostData[base_index + 0] = 0;

            //Anchor write
            PostData[base_index + 1] = anchor.x;
            PostData[base_index + 2] = -anchor.y;

            //Color write
            PostData[base_index + 3] = color.x;
            PostData[base_index + 4] = color.y;
            PostData[base_index + 5] = color.z;
            PostData[base_index + 6] = color.w;

            //Bound write
            PostData[base_index + 7] = bound.x;
            PostData[base_index + 8] = bound.y;

            //flip write
            PostData[base_index + 9] = horizon_flip ? -1 : 1;
            PostData[base_index + 10] = vertical_flip ? -1 : 1;

            //ModelMatrix write
            Matrix4 model =
                Matrix4.Identity *
            Matrix4.CreateScale(scale.x, scale.y, 1) *
            Matrix4.CreateFromAxisAngle(_staticCacheAxis, rotate) *
            Matrix4.CreateTranslation(position.x-StoryboardWindow.CurrentWindow.Width/2, -position.y+ StoryboardWindow.CurrentWindow.Height / 2, 0);
            //model.Transpose();

            int i = 0;

            _Matrix4ToFloatArray(ref model);
            foreach (var value in _cacheMatrix)
            {
                PostData[base_index + 11 + (i++)] = value;
            }

            _currentPostCount++;
            if (_currentPostCount >= Capacity)
            {
                FlushDraw();
            }
        }

        public void PostRenderCommand(Vector position, float z_orther, float rotate, Vector scale,Vector anchor, Vec4 color, bool vertical_flip, bool horizon_flip) => PostRenderCommand(position, z_orther, _bound, rotate, scale, anchor, color,vertical_flip,horizon_flip);

        void _draw()
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

            _shader.Clear();
            _shader.End();
        }

        public void FlushDraw()
        {
            _draw();
            Clear();
        }

        public void Clear()
        {
            _currentPostCount = 0;
        }

        static float[] _cacheMatrix = new float[16];
        static void _Matrix4ToFloatArray(ref Matrix4 matrix)
        {
            unsafe
            {
                fixed (float* m_ptr = &matrix.Row0.X)
                {
                    for (int i = 0; i < 16; i++)
                        _cacheMatrix[i] = *(m_ptr + i);
                }
            }
        }
    }
}

