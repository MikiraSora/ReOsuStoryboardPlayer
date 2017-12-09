using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Collections;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Runtime.Serialization;
using OpenTK;

namespace ReOsuStoryBoardPlayer
{
    [Serializable()]
    public class Shader
    {
        int vertexShader, fragmentShader, program;
        
        bool compiled = false;

        string vert;

        string frag;
        
        public string VertexProgram { get { return vert; } set { vert = value; } }
        
        public string FragmentProgram { get { return frag; } set { frag = value; } }
        
        Dictionary<string, object> _uniforms;

        public Dictionary<string, object> Uniforms { get { return _uniforms; } internal set { _uniforms = value; } }

        public void Compile()
        {
            if (compiled == false)
            {
                compiled = true;

                Uniforms = new Dictionary<string, object>();

                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
                GL.DeleteProgram(program);

                vertexShader = GL.CreateShader(ShaderType.VertexShader);
                fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

                GL.ShaderSource(vertexShader, vert);
                GL.ShaderSource(fragmentShader, frag);

                GL.CompileShader(vertexShader);
                GL.CompileShader(fragmentShader);

                if (!String.IsNullOrEmpty(GL.GetShaderInfoLog(vertexShader)))
                    Log.Error(GL.GetShaderInfoLog(vertexShader));

                if (!String.IsNullOrEmpty(GL.GetShaderInfoLog(fragmentShader)))
                    Log.Error(GL.GetShaderInfoLog(fragmentShader));

                program = GL.CreateProgram();

                GL.AttachShader(program, vertexShader);
                GL.AttachShader(program, fragmentShader);

                GL.LinkProgram(program);

                if (!String.IsNullOrEmpty(GL.GetProgramInfoLog(program)))
                    Log.Error(GL.GetProgramInfoLog(program));

                int total = 0;

                GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, out total);
                for (int i = 0; i < total; i++)
                {
                    int size = 16;
                    int name_len = 16;

                    ActiveUniformType type = ActiveUniformType.Sampler2D;
                    StringBuilder name = new StringBuilder();

                    GL.GetActiveUniform(program, i, 16, out name_len, out size, out type, name);

                    name = null;
                    size = 0;
                    name_len = 0;
                }
            }
        }

        public void Begin()
        {
            GL.UseProgram(program);
        }

        public void End()
        {
            GL.UseProgram(0);
        }

        public void PassUniform(string name, Texture tex)
        {
            if (tex==null)
            {
                PassNullTexUniform(name);
                return;
            }

            int l = GL.GetUniformLocation(program, name);
            GL.ActiveTexture(TextureUnit.Texture0 + tex.ID);
            GL.BindTexture(TextureTarget.Texture2D, tex.ID);
            GL.Uniform1(l, tex.ID);
            GL.ActiveTexture(TextureUnit.Texture0);

            AddPassRecord(name, "Texture");
        }

        public void PassNullTexUniform(string name)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Uniform1(l, 0);
        }

        public void PassUniform(string name, Vec4 vec)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.Uniform4(l, vec.x, vec.y, vec.z, vec.w);

            AddPassRecord(name, "Vec4");
        }

        public void PassUniform(string name, float val)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.Uniform1(l, val);
            
            AddPassRecord(name, "Float");
        }

        public void PassUniform(string name, int val)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.Uniform1(l, val);
            
            AddPassRecord(name, "Int");
        }

        public void PassUniform(string name, Vector2 val)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.Uniform2(l, val);
            
            AddPassRecord(name, "Vector2");
        }

        public void PassUniform(string name, OpenTK.Matrix4 matrix4)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.UniformMatrix4(l, false, ref matrix4);
            
            AddPassRecord(name, "Matrix4");
        }
        
        internal void AddPassRecord(string name,string value)
        {
            recordPassHistory.Add(new passUniformRecord()
            {
                name = name,
                value = value
            });
        }

        public void ClearUniform(string key, string typeName)
        {
            switch (typeName)
            {
                case "Sampler2D":
                    PassNullTexUniform(key);
                    break;
                case "Texture":
                    PassNullTexUniform(key);
                    break;
                case "Vec4":
                    PassUniform(key, Vec4.zero);
                    break;
                case "Float":
                    PassUniform(key, (float)0);
                    break;
                case "Int":
                    PassUniform(key, (int)0);
                    break;
                case "Int32":
                    PassUniform(key, (Int32)0);
                    break;
                case "Single":
                    PassUniform(key, (Single)0);
                    break;
                case "Vector2":
                    PassUniform(key, (Vector2.Zero));
                    break;
            }
        }

        struct passUniformRecord
        {
            internal string name;
            internal string value;
        }

        List<passUniformRecord> recordPassHistory = new List<passUniformRecord>();

        public void Clear()
        {
            foreach (var history in recordPassHistory)
            {
                ClearUniform(history.name, history.value);
            }

            recordPassHistory.Clear();
            GL.UseProgram(0);
        }

        public int ShaderProgram => program;
    }
}