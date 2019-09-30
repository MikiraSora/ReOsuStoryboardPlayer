using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace ReOsuStoryboardPlayer.Graphics
{
    public class StoryboardResource : IDisposable
    {
        public Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap { get; private set; }

        public SpriteInstanceGroup GetSprite(string key)
        {
            return CacheDrawSpriteInstanceMap.TryGetValue(key,out var group)?group:null;
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

        public static StoryboardResource BuildDefaultResource(IEnumerable<StoryboardObject> StoryboardObjectList, string folder_path)
        {
            Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap = new Dictionary<string, SpriteInstanceGroup>();

            StoryboardResource resource = new StoryboardResource();

            foreach (var obj in StoryboardObjectList)
            {
                SpriteInstanceGroup group;
                switch (obj)
                {
                    case StoryboardBackgroundObject background:
                        if (!_get(obj.ImageFilePath.ToLower(), out group))
                            Log.Warn($"not found image:{obj.ImageFilePath}");

                        if (group != null)
                        {
                            background.AdjustScale(group.Texture.Height);
                        }

                        break;

                    case StoryboardAnimation animation:
                        List<SpriteInstanceGroup> list = new List<SpriteInstanceGroup>();

                        for (int index = 0; index < animation.FrameCount; index++)
                        {
                            string path = animation.FrameBaseImagePath + index + animation.FrameFileExtension;
                            if (!_get(path, out group))
                            {
                                Log.Warn($"not found image:{path}");
                                continue;
                            }
                            list.Add(group);
                        }

                        break;

                    default:
                        if (!_get(obj.ImageFilePath.ToLower(), out group))
                            Log.Warn($"not found image:{obj.ImageFilePath}");
                        break;
                }
            }

            resource.PinSpriteInstanceGroups(CacheDrawSpriteInstanceMap);

            return resource;

            bool _get(string image_name, out SpriteInstanceGroup group)
            {
                var fix_image = image_name;
                //for Flex
                if (string.IsNullOrWhiteSpace(Path.GetExtension(fix_image)))
                    fix_image += ".png";

                if (CacheDrawSpriteInstanceMap.TryGetValue(image_name, out group))
                    return true;

                //load
                string file_path = Path.Combine(folder_path, fix_image);

                if (!_load_tex(file_path, out var tex))
                {
                    file_path = Path.Combine(PlayerSetting.UserSkinPath??string.Empty, fix_image);

                    if (!_load_tex(file_path, out tex))
                    {
                        if ((!image_name.EndsWith("-0")) && _get(image_name + "-0", out group))
                            return true;
                    }
                }

                if (tex != null)
                {
                    group = CacheDrawSpriteInstanceMap[image_name] = new SpriteInstanceGroup((uint)PlayerSetting.DrawCallInstanceCountMax, fix_image, tex);
                    Log.Debug($"Created Storyboard sprite instance from image file :{fix_image}");
                }

                return group != null;
            }

            bool _load_tex(string file_path, out Texture texture)
            {
                texture = null;

                try
                {
                    if (!File.Exists(file_path))
                        return false;

                    texture = new Texture(file_path);
                }
                catch (Exception e)
                {
                    Log.Warn($"Load texture \"{file_path}\" failed : {e.Message}");
                    texture = null;
                }

                return texture != null;
            }
        }
    }
}