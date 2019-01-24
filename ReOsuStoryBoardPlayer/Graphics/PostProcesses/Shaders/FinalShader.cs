namespace ReOsuStoryboardPlayer.Graphics.PostProcesses.Shaders
{
    internal class FinalShader : PostProcessShader
    {
        public FinalShader()
        {
            this.FragmentProgram=
@"          #version 330 core
            uniform sampler2D tex;

            in vec2 uv;

            out vec4 out_color;
            void main(){
                out_color = texture(tex,uv);
            }
";
        }
    }
}