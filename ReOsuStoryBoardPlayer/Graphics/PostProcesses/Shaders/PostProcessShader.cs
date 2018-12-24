using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Graphics.PostProcesses.Shaders
{
    class PostProcessShader:Shader
    {
        public PostProcessShader()
        {
            this.VertexProgram =
@"              #version 330 core
                layout(location=0) in vec2 in_pos;
                layout(location=1) in vec2 in_uv;

                out vec2 f_uv;
                
                void main(){
                    gl_Position = vec4(in_pos,0,1);
                    f_uv = in_uv;
                }
";
        }
    }
}
