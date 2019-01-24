using ReOsuStoryboardPlayer.Core.Base;
using System;
using System.Collections.Generic;

namespace ReOsuStoryboardPlayer.Graphics
{
    public class StoryboardResource : IDisposable
    {
        public Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap { get; private set; }

        public SpriteInstanceGroup GetSprite(string key)
        {
            return CacheDrawSpriteInstanceMap[key];
        }

        public SpriteInstanceGroup GetSprite(StoryboardObject obj) => GetSprite(obj.ImageFilePath);

        public void PinSpriteInstanceGroups(Dictionary<string, SpriteInstanceGroup> sprites)
        {
            CacheDrawSpriteInstanceMap=new Dictionary<string, SpriteInstanceGroup>(sprites.Count);

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