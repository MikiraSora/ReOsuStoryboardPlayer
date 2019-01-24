using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Utils;
using System.Collections.Generic;

namespace ReOsuStoryboardPlayer.Optimzer
{
    public abstract class OptimzerBase
    {
        public abstract void Optimze(IEnumerable<StoryboardObject> Storyboard_objects);

        public void Suggest(string message)
        {
            if (Setting.ShowProfileSuggest)
            {
                Log.User(message);
            }
        }

        public void Suggest(StoryboardObject obj, string message)
        {
            Suggest($"在line {obj.FileLine}物件\"{obj.ImageFilePath}\","+message);
        }

        public void Suggest(Command cmd, string message)
        {
            Suggest($"在line {cmd.RelativeLine}命令\"{cmd.ToString()}\","+message);
        }
    }
}