using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace ReOsuStoryBoardPlayer.Graphics.PostProcesses
{
    public abstract class APostProcess
    {
        private static float[] s_cacheBaseVertex = new float[] {
            -1.0f,  1.0f,
             1.0f,  1.0f,
             1.0f, -1.0f,
            -1.0f, -1.0f,
        };

        private static float[] s_cacheBaseTexPos = new float[] {
            0,1,
            1,1,
            1,0,
            0,0
        };

        private static int s_vbo;
        private static int s_vao;

        static APostProcess()
        {
            s_vbo = GL.GenBuffer();
            s_vao = GL.GenVertexArray();

            BuildVBO();
        }

        private static void BuildVBO()
        {
            GL.BindVertexArray(s_vao);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, s_vbo);
                {
                    //分配空间
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * (s_cacheBaseVertex.Length+s_cacheBaseTexPos.Length)),
                        IntPtr.Zero, BufferUsageHint.StaticDraw);

                    GL.BufferSubData<float>(BufferTarget.ArrayBuffer,IntPtr.Zero, new IntPtr(sizeof(float) * s_cacheBaseVertex.Length),s_cacheBaseVertex);
                    GL.BufferSubData<float>(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * s_cacheBaseVertex.Length), new IntPtr(sizeof(float) * s_cacheBaseTexPos.Length), s_cacheBaseTexPos);

                    GL.EnableVertexAttribArray(0);
                    GL.EnableVertexAttribArray(1);

                    GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
                    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, new IntPtr(sizeof(float) * s_cacheBaseVertex.Length));
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.BindVertexArray(0);
        }

        public PostProcessFrameBuffer PrevFrameBuffer;

        protected abstract void OnUseShader();
        protected virtual void OnPreRender() { }
        protected virtual void OnPostRender() { }

        public void Process()
        {
            OnUseShader();
            GL.BindVertexArray(s_vao);
            {
                OnPreRender();
                GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
                OnPostRender();
            }
            GL.BindVertexArray(0);
        }
    }
}
