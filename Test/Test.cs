using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleRenderFramework;

namespace Test
{
	public class Test
	{        
		public static void Main(string[] argv)
		{
            TestGameWindow s = new TestGameWindow();

            Engine.Debug = true;

            s.resizeWindow(640, 480);
            s.Run();
        }
	}
}
