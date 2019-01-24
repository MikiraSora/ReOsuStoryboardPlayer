using OpenTK;
using OpenTK.Graphics.OpenGL;
using ReOsuStoryBoardPlayer.Core.PrimitiveValue;
using System;

namespace ReOsuStoryBoardPlayer.Graphics
{
    //from https://github.com/MikiraSora/SimpleRenderFramework/blob/redesign/SimpleRenderFramework/Graphics/Drawing.cs
    public static class DrawUtils
    {
        private static int texture_vao, texture_vbo_pos, texture_vbo_tex;

        private static Shader texture_shader;

        private static readonly Vector3 _staticCacheAxis = new Vector3(0, 0, 1);

        public static void Init()
        {
            #region Texture

            texture_vao=GL.GenVertexArray();
            texture_vbo_pos=GL.GenBuffer();
            texture_vbo_tex=GL.GenBuffer();

            GL.BindVertexArray(texture_vao);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, texture_vbo_pos);
                {
                    float[] data = {
                        0,0,
                        0,-1,
                        1,-1,
                        1,0,
                    };

                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(2*4*sizeof(float)), data, BufferUsageHint.StaticDraw);

                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2*sizeof(float), 0);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, texture_vbo_tex);
                {
                    float[] data = {
                         0,0,
                        0,-1,
                        1,-1,
                        1,0,
                    };

                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(2*4*sizeof(float)), data, BufferUsageHint.StaticDraw);

                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2*sizeof(float), 0);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.BindVertexArray(0);

            texture_shader=new Shader()
            {
                VertexProgram=@"
								#version 330
out vec4 varying_color;
out vec2 varying_texPos;
uniform mat4 ViewProjection;
uniform vec2 in_anchor;
uniform vec2 in_bound;
uniform mat4 in_model;
layout(location=0) in vec2 in_texPos;
layout(location=1) in vec2 in_pos;
void main(){
	gl_Position=ViewProjection*in_model*vec4((in_pos-in_anchor)*in_bound,0,1.0);
	varying_texPos=in_texPos;
}
",
                FragmentProgram=@"
#version 330
uniform sampler2D diffuse;
uniform vec4 Color;
in vec2 varying_texPos;
out vec4 out_color;
void main(){
	vec4 texColor=texture(diffuse,vec2(-varying_texPos.x,varying_texPos.y));
	out_color=(Color*texColor);
}
"
            };
            texture_shader.Compile();

            #endregion Texture
        }

        public static void DrawTexture(Texture texture, Vector position, Vector norAnchor, Vector scale, int angle, int width, int height, Vec4 color)
        {
            texture_shader.Begin();
            var VP = StoryboardWindow.ProjectionMatrix*StoryboardWindow.CameraViewMatrix;

            Matrix4 model =
                Matrix4.Identity*
                Matrix4.CreateScale(scale.X, scale.Y, 1)*
                Matrix4.CreateTranslation(position.X, -position.Y, 0)*
                Matrix4.CreateFromAxisAngle(_staticCacheAxis, angle/180.0f*3.1415926f)
            ;

            Vector2 anchorFixer = new Vector2(norAnchor.X, -norAnchor.Y);

            //Pass Uniform Variable
            texture_shader.PassUniform("ViewProjection", VP);
            texture_shader.PassUniform("diffuse", texture);
            texture_shader.PassUniform("Color", color);
            texture_shader.PassUniform("in_anchor", anchorFixer);
            texture_shader.PassUniform("in_bound", new Vector2(width, height));
            texture_shader.PassUniform("in_model", model);

            GL.BindVertexArray(texture_vao);
            {
                GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            }
            GL.BindVertexArray(0);

            texture_shader.End();
        }
    }
}