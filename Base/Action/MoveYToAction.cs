using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGLF;

namespace OsuStoryBroadPlayer
{
    public class MoveYToAction : ActionBase
    {
        int endY, startY;

        public MoveYToAction(GameObject gameObject, int endY, float time, IInterpolator interpolatior) : this(gameObject,(int)gameObject.LocalPosition.y, endY, time, interpolatior) { }


        public MoveYToAction(GameObject gameObject, int startY, int endY, float time, IInterpolator interpolatior) : base(0, time, interpolatior, gameObject)
            {
            this.endY = endY;

            this.startY = startY;
        }

        public override void onUpdate(float norValue)
        {
            if (norValue >= 1)
            {
                markDone();
                gameObject.LocalPosition = new Vector(gameObject.LocalPosition.x, endY);
            }

            float y = norValue * (endY - startY);
            //Console.WriteLine("time : {0:F2}\tx : {1:F2}\ty :　{2:F2}",passTime,x,y);

            gameObject.LocalPosition = new Vector( gameObject.LocalPosition.x,y + startY);
        }

        public override ActionBase reverse()
        {
            return new MoveXToAction(gameObject, endY, startY, _timeEnd - _timeStart, interpolator.reverse());
        }
    }
}
