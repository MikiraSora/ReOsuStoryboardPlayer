using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Graphics.PostProcesses.Shaders
{
    class FinalShader:PostProcessShader
    {
        public FinalShader()
        {
            this.FragmentProgram =
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
