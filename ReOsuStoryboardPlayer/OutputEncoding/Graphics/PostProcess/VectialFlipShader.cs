namespace ReOsuStoryboardPlayer.Graphics.PostProcesses.Shaders
{
    internal class VectialFlipShader : PostProcessShader
    {
        public VectialFlipShader()
        {
            this.FragmentProgram=
@"          #version 330 core
            uniform sampler2D tex;

            in vec2 uv;

            out vec4 out_color;
            void main(){
                vec2 new_uv=vec2(uv.x,1-uv.y);

                out_color = texture(tex,new_uv);
            }
";
        }
    }
}