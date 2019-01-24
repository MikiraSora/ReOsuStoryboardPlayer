using ReOsuStoryBoardPlayer.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Graphics
{
    public class StoryboardResource:IDisposable
    {
        public Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap { get; private set; } = new Dictionary<string, SpriteInstanceGroup>();
        
        public SpriteInstanceGroup GetSprite(string key)
        {
            return CacheDrawSpriteInstanceMap[key];
        }

        public SpriteInstanceGroup GetSprite(StoryBoardObject obj) => GetSprite(obj.ImageFilePath);


        public void AddSpriteInstanceGroups(Dictionary<string, SpriteInstanceGroup> sprites)
        {
            foreach (var sprite in sprites)
                CacheDrawSpriteInstanceMap.Add(sprite.Key, sprite.Value);
        }

        public void Dispose()
        {
            foreach (var item in CacheDrawSpriteInstanceMap)
            {
                item.Value.Dispose();
            }

            CacheDrawSpriteInstanceMap.Clear();
        }
    }
}
