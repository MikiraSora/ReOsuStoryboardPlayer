using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace ReOsuStoryboardPlayer.Utils
{
    public static class DebugCheck
    {
        public static void CheckOpenGL()
        {
            try
            {
                var error = GL.GetError();
                Debug.Assert(error == ErrorCode.NoError, $"GL.GetError() -> {error}");
            }
            catch (Exception e)
            {

            }
        }
    }
}
