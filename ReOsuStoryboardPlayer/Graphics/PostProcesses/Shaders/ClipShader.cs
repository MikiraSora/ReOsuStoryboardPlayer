namespace ReOsuStoryboardPlayer.Graphics.PostProcesses.Shaders
{
    internal class ClipShader : PostProcessShader
    {
        public ClipShader()
        {
            this.FragmentProgram=
@"          #version 330 core
            #define SB_WIDTH 640
            #define SB_HEIGHT 480

            uniform sampler2D tex;
            uniform float view_width;

            in vec2 uv;

            out vec4 out_color;
            void main(){
                float offset = (view_width - SB_WIDTH)*0.5 / view_width;
                if(uv.x < offset || uv.x > (1.0 - offset))
                    out_color = vec4(0,0,0,1);
                else
                    out_color = texture(tex,uv);
            }
";
        }
    }
}