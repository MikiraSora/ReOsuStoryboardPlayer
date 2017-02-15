using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGLF;

namespace OsuStoryBroadPlayer
{
    class MoveXToAction :ActionBase
    {
        int endX, startX;

        public MoveXToAction(GameObject gameObject, int endX, float time, IInterpolator interpolatior) : this(gameObject, (int)gameObject.LocalPosition.x, endX, time, interpolatior) { }


        public MoveXToAction(GameObject gameObject, int startX, int endX, float time, IInterpolator interpolatior) : base(0, time, interpolatior,gameObject)
        {
            this.endX = endX;

            this.startX = startX;
        }

        public override void onUpdate(float norValue)
        {
            if (norValue >= 1)
            {
                markDone();
                gameObject.LocalPosition = new Vector(endX, gameObject.LocalPosition.y);
            }

            float x = norValue * (endX - startX);
            //Console.WriteLine("time : {0:F2}\tx : {1:F2}\ty :　{2:F2}",passTime,x,y);

            gameObject.LocalPosition = new Vector(x + startX, gameObject.LocalPosition.y);
        }

        public override ActionBase reverse()
        {
            return new MoveXToAction(gameObject, endX, startX, _timeEnd - _timeStart, interpolator.reverse());
        }
    }
}
