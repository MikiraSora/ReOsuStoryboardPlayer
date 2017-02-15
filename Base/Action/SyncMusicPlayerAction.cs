using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGLF;
using IrrKlang;

namespace OsuStoryBroadPlayer
{
    class SyncMusicPlayerAction:ActionBase
    {
        StoryBroadInitializer _initializer = null;
        SBSpriteGameObject gameobject;
        long _trigger = 0;

        public SyncMusicPlayerAction(SBSpriteGameObject gameobject,StoryBroadInitializer initializer,long trigger):base(0,float.MaxValue,null,gameobject)
        {
            _initializer = initializer;
            _trigger = trigger;
            this.gameobject = gameobject;
        }

        public override void onAction(float passTime)
        {
            onUpdate(0);
        }

        public override void onUpdate(float norValue)
        {   
            if(_initializer._currentPlayer.PlayPosition>=_trigger)
            {
                Log.User("PlayBack:{0} sprite {1} appeared! in (gameobject {2} - trigger {3})", _initializer._currentPlayer.PlayPosition,gameobject.name,gameobject._startTime,_trigger);
                markDone();
            }
        }
    }
}
