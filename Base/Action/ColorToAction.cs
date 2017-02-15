using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGLF;

namespace OsuStoryBroadPlayer
{
    public class ColorToAction : ActionBase
    {
        float _startR, _startG, _startB, _endR, _endG, _endB;

        public ColorToAction(GameObject gameobject, float endR, float endG, float endB, float time, IInterpolator interpolator) : this(gameobject, gameobject.sprite.getColor().x, gameobject.sprite.getColor().y, gameobject.sprite.getColor().z, endR,endG,endB, time, interpolator) { }

        public ColorToAction(GameObject gameobject, float startR,float startG,float startB,float endR,float endG,float endB, float time, IInterpolator interpolator) : base(0, time, interpolator, gameobject)
        {
            _startB = startB;
            _startG = startG;
            _startR = startR;

            _endB = endB;
            _endG = endG;
            _endR = endR;
        }

        public override void onUpdate(float norValue)
        {
            if (norValue >= 1)
            {
                markDone();
                gameObject.sprite.setColor(_endR,_endG,_endB, gameObject.sprite.getColor().w);
            }

            float r = norValue * (_endR - _startR);
            float g = norValue * (_endG - _startG);
            float b = norValue * (_endB - _startB);

            gameObject.sprite.setColor(_startR + r, _startG + g, _startB + b, gameObject.sprite.getColor().w);
        }

        public override ActionBase reverse()
        {
            return new ColorToAction(gameObject, _endR,_endG,_endB,_startR,_startG,_startB, _timeEnd - _timeStart, interpolator.reverse());
        }
    }
}
