using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGLF;
using OpenGLF_EX;
using StoryBroadParser;

namespace OsuStoryBroadPlayer
{
    public class SpriteGameObject : GameObject
    {
        public StoryBroadParser.Sprite _sbSprite;

        public StoryBroadInitializer _initializer;

        public ActionBase _commandsActions;
        
        public SpriteGameObject(string _oszFilePath, ref StoryBroadParser.Sprite sprite,ActionBase action,StoryBroadInitializer initializer)
        {
            _sbSprite = sprite;
            _commandsActions = action;
            _initializer = initializer;

            components.Add(new ActionExecutor());


            //setup image
            TextureSprite imaSprite = new TextureSprite(TextureManager.cacheTexture(_oszFilePath + sprite._imgPath, name => { return new Texture(name); }));
            //setup anchor
            switch (sprite._origin)
            {
                case Origin.TopLeft:
                    imaSprite.center = new Vector(0, 0);
                    break;
                case Origin.TopCentre:
                    imaSprite.center = new Vector(imaSprite.Texture.bitmap.Width / 2, 0);
                    break;
                case Origin.TopRight:
                    imaSprite.center = new Vector(imaSprite.Texture.bitmap.Width, 0);
                    break;
                case Origin.CentreLeft:
                    imaSprite.center = new Vector(0, imaSprite.Texture.bitmap.Height / 2);
                    break;
                case Origin.Centre:
                    imaSprite.center = new Vector(imaSprite.Texture.bitmap.Width / 2, imaSprite.Texture.bitmap.Height / 2);
                    break;
                case Origin.CentreRight:
                    imaSprite.center = new Vector(imaSprite.Texture.bitmap.Width, imaSprite.Texture.bitmap.Height / 2);
                    break;
                case Origin.BottomLeft:
                    imaSprite.center = new Vector(0, imaSprite.Texture.bitmap.Height);
                    break;
                case Origin.BottomCentre:
                    imaSprite.center = new Vector(imaSprite.Texture.bitmap.Width / 2, imaSprite.Texture.bitmap.Height);
                    break;
                case Origin.BottomRight:
                    imaSprite.center = new Vector(imaSprite.Texture.bitmap.Width, imaSprite.Texture.bitmap.Height);
                    break;
                default:
                    break;
            }

            imaSprite.width = imaSprite.Texture.bitmap.Width;
            imaSprite.height = imaSprite.Texture.bitmap.Height;
            components.Add(imaSprite);
            this.name = sprite._imgPath;
        }

        public void Show(long currentPlayBackTime)
        {
            Engine.scene.GameObjectRoot.addChild(this);
            var offset = _sbSprite._minStartTime - currentPlayBackTime;
            if (offset < 0)
                offset = 0;

            //todo : wrap wait time end auto dispose gameobject and resource
            Log.User("Sprite {0} is loaded and shown.{1}:{2}-{3}",name,currentPlayBackTime,_sbSprite._minStartTime, _sbSprite._maxEndTime);

            var actionCommand = _initializer.buildSpriteCommand(this, ref _sbSprite);
            getComponent<ActionExecutor>().executeAction(new FrameAction(new ActionBase[] {
                new WaitAction(offset),
                actionCommand,
                new SpriteGameObjectDistoryAction(this)
            }));
        }
    }
}
