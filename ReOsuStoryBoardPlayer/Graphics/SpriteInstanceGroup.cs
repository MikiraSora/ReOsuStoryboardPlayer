using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using OpenTK;
using System.Threading.Tasks;
using SimpleRenderFramework;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;

namespace ReOsuStoryBoardPlayer
{
    public class SpriteInstanceGroup
    {
        public uint Capacity { get; protected set; } = 0;

        int _currentPostCount = 0;

        static DebugBatchShader _debug_shader;
        static BatchShader _shader;

        public static Matrix4 Projection { get { return Engine.ProjectionMatrix; } }
        public static Matrix4 View { get { return Engine.CameraViewMatrix; } }

        public int CurrentPostCount { get => _currentPostCount; }

        int _vao, _vbo, _vbo_vertexBase, _vbo_texPosBase;

        InstanceData[] _instanceDataArray;

        private SpriteInstanceGroup() { }

        public Material _material;

        public string ImagePath { get; private set; }

        static SpriteInstanceGroup()
        {
            _shader = new BatchShader();
            _shader.compile();

            _debug_shader = new DebugBatchShader();
            _debug_shader.compile();
        }

        internal SpriteInstanceGroup(uint capacity,string image_path, Texture texture)
        {
            ImagePath = image_path;

            _bound.x = texture.Width;
            _bound.y = texture.Height;

            this.Capacity = capacity;
            _material = new Material();
            
            _material.shader = _shader;

            _material.parameters["diffuse"] = texture;

            _buildBuffer();

            _staticCacheData = new float[_calculateCapacitySize()];
            _instanceDataArray = new InstanceData[capacity];
        }

        ~SpriteInstanceGroup()
        {
            _deleteBuffer();
        }

        int _calculateCapacitySize()
        {
            /*-----------------CURRENT VERSION------------------ -
					*orther		anchor	    color		bound       modelMatrix
					*float(1)	vec2(2)		vec4(4)     vec2(2)     Matrix4(16)
					*/
            return (1 + 2 + 4 + 2 + 16) * sizeof(float);
        }

        Vector _bound,_anchor=new Vector(0.5f,0.5f);

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

                    /*-----------------CURRENT VERSION------------------ -
					*orther		anchor	    color		bound       modelMatrix
					*float(1)	vec2(2)		vec4(4)     vec2(2)     Matrix4(16)
					*/

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

                    //ModelMatrix
                    GL.EnableVertexAttribArray(6);
                    GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 36);
                    GL.VertexAttribDivisor(6, 1);
                    GL.EnableVertexAttribArray(7);
                    GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 52);
                    GL.VertexAttribDivisor(7, 1);
                    GL.EnableVertexAttribArray(8);
                    GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 68);
                    GL.VertexAttribDivisor(8, 1);
                    GL.EnableVertexAttribArray(9);
                    GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, _calculateCapacitySize(), 84);
                    GL.VertexAttribDivisor(9, 1);
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

        float[] _staticCacheData;

        public void PostRenderCommand(Vector position, float z_other, Vector bound, float rotate, Vector scale, Vector anchor, Vec4 color)
        {
            /*-----------------CURRENT VERSION------------------ -
			*orther		anchor	    color		bound       modelMatrix
			*float(1)	vec2(2)		vec4(4)     vec2(2)     Matrix4(16)
			*/
            if (_instanceDataArray[_currentPostCount] == null)
                _instanceDataArray[_currentPostCount] = new InstanceData();
            var instanceData = _instanceDataArray[_currentPostCount];
            if (instanceData.data == null)
                instanceData.data = new float[_calculateCapacitySize()];
            var data = instanceData.data;

            //Z float
            data[0] = -z_other;

            //Anchor write
            data[1] = anchor.x;
            data[2] = -anchor.y;

            //Color write
            data[3] = color.x;
            data[4] = color.y;
            data[5] = color.z;
            data[6] = color.w;

            //Bound write
            data[7] = bound.x;
            data[8] = bound.y;

            //ModelMatrix write
            Matrix4 model =
                Matrix4.Identity *
            Matrix4.CreateScale(scale.x, scale.y, 1) *
            Matrix4.CreateFromAxisAngle(_staticCacheAxis, rotate / 180.0f * 3.1415926f) *
            Matrix4.CreateTranslation(position.x-Window.CurrentWindow.Width/2, -position.y+ Window.CurrentWindow.Height / 2, 0);
            //model.Transpose();

            int i = 0;

            _Matrix4ToFloatArray(ref model);
            foreach (var value in _cacheMatrix)
            {
                data[9 + (i++)] = value;
            }

            _currentPostCount++;
            if (_currentPostCount >= Capacity)
            {
                FlushDraw();
            }
        }

        public void PostRenderCommand(Vector position, float z_orther, float rotate, Vector scale,Vector anchor, Vec4 color) => PostRenderCommand(position, z_orther, _bound, rotate, scale, anchor, color);

        class InstanceData : IComparable
        {
            public float[] data;
            public float z { get { return data == null ? 0 : data[0]; } }

            public int CompareTo(object obj)
            {
                var other = ((InstanceData)obj);
                var result = (other).z - z;
                return result < 0 ? -1 : result > 0 ? 1 : 0;
            }
        }

        Matrix4 offset_view = Matrix4.CreateTranslation(new Vector3(-Window.CurrentWindow.Width / 2, -Window.CurrentWindow.Height / 2, 0));

        void _draw()
        {
            if (Engine.Debug)
                _material.shader = _debug_shader;
            else
                _material.shader = _shader;

            _material.shader.begin();
            var VP = Projection * (View);
            foreach (var pair in _material.parameters)
            {
                if (pair.Value != null)
                    _material.shader.PassUniform(pair.Key, pair.Value);
            }

            _material.shader.PassUniform("ViewProjection", VP);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            {
                for (int i = 0; i < _currentPostCount; i++)
                {
                    GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(i * _calculateCapacitySize()), (IntPtr)_instanceDataArray[i].data.Length, _instanceDataArray[i].data);
                }  
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(_vao);
            {
                GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, _currentPostCount);
            }

            GL.BindVertexArray(0);
            
            for (int i = 0; i < _currentPostCount; i++)
            {
                _instanceDataArray[i].data[0] = -1000000;
            }

            _material.shader.Clear();
            _material.shader.end();
        }

        public void FlushDraw()
        {
            Array.Sort(_instanceDataArray, 0, _currentPostCount);
            
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

