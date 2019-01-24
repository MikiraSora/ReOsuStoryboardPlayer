namespace ReOsuStoryboardPlayer.Graphics.PostProcesses.Shaders
{
    internal class PostProcessShader : Shader
    {
        public PostProcessShader()
        {
            this.VertexProgram=
@"              #version 330 core
                layout(location=0) in vec2 in_pos;
                layout(location=1) in vec2 in_uv;

                out vec2 uv;

                void main(){
                    gl_Position = vec4(in_pos,0,1);
                    uv = in_uv;
                }
";
        }
    }
}