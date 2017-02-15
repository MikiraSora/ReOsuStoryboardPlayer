using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGLF;

namespace OsuStoryBroadPlayer
{
    public class SpriteGameObjectDistoryAction : ImmediatelyActionBase
    {
        public SpriteGameObjectDistoryAction(SBSpriteGameObject gameobject) : base(gameobject) { }

        public override void onUpdate()
        {
            Schedule.addMainThreadUpdateTask(new Schedule.ScheduleTask(0, (refTask, param) => {
                Engine.scene.GameObjectRoot.removeChild(gameObject);
            }, null));
            Log.User("Sprite {0} was distory!",((SBSpriteGameObject)gameObject).name);
        }
    }
}
