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
        [NonSerialized]
        int vertexShader, fragmentShader, program;

        [NonSerialized]
        bool compiled = false;

        string vert =
        "void main(void)" +
        "{" +
            "gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;" +
            "gl_Position = ftransform();" +
        "}";

        string frag =
        "uniform sampler2D diffuse;" +
                "uniform vec4 colorkey;" +
                "void main(void)" +
                "{" +
                    "vec4 color = texture2D(diffuse, gl_TexCoord[0].st);" +
                    "gl_FragColor = color * colorkey;" +
                "}";
        //[Editor(typeof(StringEditor), typeof(UITypeEditor))]
        [Browsable(false)]
        public string vertexProgram { get { return vert; } set { vert = value; } }

        //[Editor(typeof(StringEditor), typeof(UITypeEditor))]
        [Browsable(false)]
        public string fragmentProgram { get { return frag; } set { frag = value; } }

        [NonSerialized]
        Dictionary<string, object> _uniforms;

        public Dictionary<string, object> uniforms { get { return _uniforms; } internal set { _uniforms = value; } }

        string name;

        public Shader()
        {
            name = "Shader";
        }

        public void compile()
        {
            if (compiled == false)
            {
                compiled = true;

                uniforms = new Dictionary<string, object>();

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
                else
                    Log.Debug("Shader \"" + name + "\" successfully compiled");

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

        public void fullRecompile()
        {
            compiled = false;
            compile();
        }

        public void begin()
        {
            GL.UseProgram(program);
        }

        public void end()
        {
            GL.UseProgram(0);
        }

        internal void _passUniform(string name, Texture tex)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.ActiveTexture(TextureUnit.Texture0 + tex.ID);
            GL.BindTexture(TextureTarget.Texture2D, tex.ID);
            GL.Uniform1(l, tex.ID);
            GL.ActiveTexture(TextureUnit.Texture0);
            l = 0;
        }

        internal void _passNullTexUniform(string name)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Uniform1(l, 0);
            l = 0;
        }

        internal void _passUniform(string name, Vec4 vec)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.Uniform4(l, vec.x, vec.y, vec.z, vec.w);
            l = 0;
        }

        internal void _passUniform(string name, float val)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.Uniform1(l, val);
            l = 0;
        }

        internal void _passUniform(string name, int val)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.Uniform1(l, val);
            l = 0;
        }

        internal void _passUniform(string name, Vector2 val)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.Uniform2(l, val);
            l = 0;
        }

        internal void _passUniform(string name, OpenTK.Matrix4 matrix4)
        {
            int l = GL.GetUniformLocation(program, name);
            GL.UniformMatrix4(l, false, ref matrix4);
            l = 0;
        }

        public void PassUniform(string _key, object _value)
        {
            switch (_value)
            {
                case Texture texture:
                    if (texture == null)
                        _passNullTexUniform(_key);
                    else
                        _passUniform(_key, texture);
                    break;
                case Vec4 v4:
                    _passUniform(_key, v4);
                    break;
                case float f:
                    _passUniform(_key, f);
                    break;
                case int i:
                    _passUniform(_key, i);
                    break;
                case Matrix4 m:
                    _passUniform(_key, m);
                    break;
                case Vector v:
                    _passUniform(_key, new Vector2(v.x, v.y));
                    break;
                case Vector2 v2:
                    _passUniform(_key, v2);
                    break;
                default:
                    Log.Warn("type {0} cant pass to shader uniform", _key);
                    break;
            }

            recordPassHistory.Add(new passUniformRecord()
            {
                name = _key,
                value = _value.GetType().Name
            });
        }

        public void passClearUniform(string key, string typeName)
        {
            switch (typeName)
            {
                case "Sampler2D":
                    _passNullTexUniform(key);
                    break;
                case "Texture":
                    _passNullTexUniform(key);
                    break;
                case "Vec4":
                    _passUniform(key, Vec4.zero);
                    break;
                case "Float":
                    _passUniform(key, (float)0);
                    break;
                case "Int":
                    _passUniform(key, (int)0);
                    break;
                case "Int32":
                    _passUniform(key, (Int32)0);
                    break;
                case "Single":
                    _passUniform(key, (Single)0);
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
                passClearUniform(history.name, history.value);
            }

            recordPassHistory.Clear();
            GL.UseProgram(0);
        }

        public int getProgram()
        {
            return program;
        }

        public override string ToString()
        {
            return name;
        }

        [OnDeserializedAttribute()]
        private void onDeserialized(StreamingContext context)
        {
            //compile();
            //Console.WriteLine("Deserialized");
        }
    }
}