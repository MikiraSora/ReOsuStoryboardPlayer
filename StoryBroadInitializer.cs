using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoryBroadParser;
using OpenGLF;
using System.Drawing;
using OpenGLF_EX;
using IrrKlang;

namespace OsuStoryBroadPlayer
{
    public class StoryBroadInitializer
    {
        List<StoryBroadParser.Sprite> _spriteList;

        string _oszFilePath;

        public ISound _currentPlayer;

        public StoryBroadInitializer(string oszFilePath,List<StoryBroadParser.Sprite> spriteList)
        {
            _spriteList = spriteList;
            _oszFilePath = oszFilePath;
        }

        public void SetPlayer(ISound player)
        {
            _currentPlayer = player;
        }

        public List<SBSpriteGameObject> Genarate()
        {
            SBSpriteGameObject gameObject;
            TextureSprite sprite;
            StoryBroadParser.Sprite SBSprite;

            List<SBSpriteGameObject> outputList = new List<SBSpriteGameObject>();

            foreach (var spriteObject in _spriteList)
            {
                gameObject = new SBSpriteGameObject();

                SBSprite = spriteObject;

                //setup image
                sprite = new TextureSprite(TextureManager.cacheTexture(_oszFilePath + spriteObject._imgPath,name=> { return new Texture(name); }));
                //setup anchor
                switch (spriteObject._origin)
                {
                    case Origin.TopLeft:
                        sprite.center = new Vector(0, 0);
                        break;
                    case Origin.TopCentre:
                        sprite.center = new Vector(sprite.Texture.bitmap.Width / 2, 0);
                        break;
                    case Origin.TopRight:
                        sprite.center = new Vector(sprite.Texture.bitmap.Width, 0);
                        break;
                    case Origin.CentreLeft:
                        sprite.center = new Vector(0, sprite.Texture.bitmap.Height / 2);
                        break;
                    case Origin.Centre:
                        sprite.center = new Vector(sprite.Texture.bitmap.Width / 2, sprite.Texture.bitmap.Height / 2);
                        break;
                    case Origin.CentreRight:
                        sprite.center = new Vector(sprite.Texture.bitmap.Width, sprite.Texture.bitmap.Height / 2);
                        break;
                    case Origin.BottomLeft:
                        sprite.center = new Vector(0, sprite.Texture.bitmap.Height);
                        break;
                    case Origin.BottomCentre:
                        sprite.center = new Vector(sprite.Texture.bitmap.Width/2, sprite.Texture.bitmap.Height);
                        break;
                    case Origin.BottomRight:
                        sprite.center = new Vector(sprite.Texture.bitmap.Width, sprite.Texture.bitmap.Height);
                        break;
                    default:
                        break;
                }

                sprite.width = sprite.Texture.bitmap.Width;
                sprite.height = sprite.Texture.bitmap.Height;

                gameObject.name = spriteObject._imgPath;

                gameObject.components.Add(sprite);
                gameObject.components.Add(new ActionExecutor());

#if DEBUG
                //gameObject.components.Add(new Selectable());
#endif

                gameObject.LocalPosition = new Vector(spriteObject._x, spriteObject._y);

                outputList.Add(gameObject);

                //Engine.scene.GameObjectRoot.addChild(gameObject);

                gameObject.getComponent<ActionExecutor>().executeAction(buildSpriteCommand(ref gameObject,ref SBSprite));
            }

            return outputList;
        }

        struct SBActions
        {
            public StoryBroadParser.Sprite sprite;
            public Command command;
            public ActionBase action;
        }

        public ActionBase buildSpriteCommand(ref SBSpriteGameObject gameObject, ref StoryBroadParser.Sprite spriteObject)
        {
            IInterpolator interpolator = null;
            List<SBActions> action_list = new List<SBActions>();

            ActionBase action=null;

            int prev_x=(int)gameObject.LocalPosition.x, prev_y=(int)gameObject.LocalPosition.y;

            float prev_w = gameObject.sprite.scale.x, prev_h = gameObject.sprite.scale.y, startScale = 0, endScale = 0,startAngle,endAngle;

            Vector startScaleVector, endScaleVector;

            float prev_fade = 1;

            Vec4 startColor, endColor;

            spriteObject._commands.Sort();

            SBActions sbAction;

            foreach (var command in spriteObject._commands)
            {

                interpolator = getEasing(command._easing);

                switch (command._event)
                {
                    case Events.Move:
                        action = new MoveToAction(gameObject,
                            (int)Convert.ToSingle(command._params[0]), (int)Convert.ToSingle(command._params[1]),
                            (int)Convert.ToSingle(command._params[2]), (int)Convert.ToSingle(command._params[3]),
                            command._endTime-command._startTime,interpolator);
                        break;

                    case Events.Fade:
                        action = new FadeToAction(gameObject,
                            Convert.ToSingle(command._params[0]),Convert.ToSingle(command._params[1]),
                            command._endTime - command._startTime
                            , interpolator);
                        break;

                    case Events.Scale:
                        startScale = Convert.ToSingle(command._params[0]);
                        endScale = Convert.ToSingle(command._params[1]);
                        action = new ScaleToAction(gameObject,new Vector(startScale,startScale),new Vector(endScale,endScale),
                            command._endTime - command._startTime
                            , interpolator);
                        break;

                    case Events.VectorScale:
                        startScaleVector = new Vector(Convert.ToSingle(command._params[0]), Convert.ToSingle(command._params[1]));
                        endScaleVector = new Vector(Convert.ToSingle(command._params[2]), Convert.ToSingle(command._params[3]));
                        action = new ScaleToAction(gameObject, startScaleVector,endScaleVector,
                            command._endTime - command._startTime
                            , interpolator);
                        break;

                    case Events.Rotate:
                        startAngle = Convert.ToSingle(command._params[0]);
                        endAngle = Convert.ToSingle(command._params[1]);
                        action = new RotateToAction(gameObject, startAngle, endAngle, command._endTime - command._startTime, interpolator);
                        break;

                    case Events.Color:
                        action = new ColorToAction(gameObject, 
                            Convert.ToSingle(command._params[0])/255.0f, Convert.ToSingle(command._params[1])/255.0f, Convert.ToSingle(command._params[2]) / 255.0f,
                            Convert.ToSingle(command._params[3]) / 255.0f, Convert.ToSingle(command._params[4]) / 255.0f, Convert.ToSingle(command._params[5]) / 255.0f,
                            command._endTime - command._startTime,interpolator);
                        break;

                    case Events.Parameter:
                        break;
                    case Events.MoveX:
                        action = new MoveXToAction(gameObject,
                            (int)Convert.ToSingle(command._params[0]), (int)Convert.ToSingle(command._params[1]),
                            command._endTime - command._startTime, interpolator);
                        break;
                    case Events.MoveY:
                        action = new MoveYToAction(gameObject,
                            (int)Convert.ToSingle(command._params[0]), (int)Convert.ToSingle(command._params[1]),
                               command._endTime - command._startTime, interpolator);
                        break;
                    case Events.Loop:
                        break;
                    case Events.Trigger:
                        break;
                    default:
                        throw new Exception("unknown event type");
                }

                sbAction = new SBActions();
                sbAction.action = action;
                sbAction.command = command;
                sbAction.sprite = spriteObject;

                action_list.Add(sbAction);
            }

            int waitOffset = spriteObject._commands.Count != 0 ? spriteObject._commands[0]._startTime<0?0- spriteObject._commands[0]._startTime:0 : 0;

            Command prev_command=null;

            gameObject.sprite.setColor(1, 1, 1, 0);

            Dictionary<Type, List<SBActions>> map = new Dictionary<Type, List<SBActions>>();

            Type type;

            foreach(SBActions sbaction in action_list)
            {
                type = sbaction.action.GetType();
                if (!map.ContainsKey(type))
                    map.Add(type, new List<SBActions>());

                map[sbaction.action.GetType()].Add(sbaction);
            }

            List<ActionBase> result = new List<ActionBase>();

            int minStartTime = int.MinValue;

            foreach (var list in map)
            {
                //Multi-Command process

                List<ActionBase> actionbaseList = new List<ActionBase>();

                //Pick all command up;
                for (int i=0;i<list.Value.Count;i++)
                {
                    sbAction = list.Value[i];
                    int offsetTime;
                    if (i != 0)
                    {
                        offsetTime =Math.Abs(list.Value[i - 1].command._endTime - sbAction.command._startTime);
                        actionbaseList.Add(new WaitAction(offsetTime));
                    }
                    else
                    {
                        offsetTime = sbAction.command._startTime;
                        actionbaseList.Add(new SyncMusicPlayerAction(gameObject,this,offsetTime));
                        if (minStartTime == int.MinValue)
                            minStartTime = offsetTime;
                        else if (minStartTime > offsetTime)
                            minStartTime = offsetTime;
                    }  

                    actionbaseList.Add(sbAction.action);
                }

                result.Add(new FrameAction(actionbaseList));
            }

            if (result.Count == 0)
                return new WaitAction(0);

            gameObject._startTime = minStartTime;

            return new FrameAction(new ActionBase[] { new ComboAction(false, result),new SpriteGameObjectDistoryAction(gameObject)});
        }

        public IInterpolator getEasing(Easing easing)
        {
            return new LinearInterpolator();
        }
    }
}
