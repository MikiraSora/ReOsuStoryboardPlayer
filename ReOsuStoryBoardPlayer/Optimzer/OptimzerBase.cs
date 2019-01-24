using ReOsuStoryBoardPlayer.Core.Base;
using ReOsuStoryBoardPlayer.Core.Commands;
using ReOsuStoryBoardPlayer.Core.Utils;
using System.Collections.Generic;

namespace ReOsuStoryBoardPlayer.Optimzer
{
    public abstract class OptimzerBase
    {
        public abstract void Optimze(IEnumerable<StoryBoardObject> storyboard_objects);

        public void Suggest(string message)
        {
            if (Setting.ShowProfileSuggest)
            {
                Log.User(message);
            }
        }

        public void Suggest(StoryBoardObject obj, string message)
        {
            Suggest($"在line {obj.FileLine}物件\"{obj.ImageFilePath}\","+message);
        }

        public void Suggest(Command cmd, string message)
        {
            Suggest($"在line {cmd.RelativeLine}命令\"{cmd.ToString()}\","+message);
        }
    }
}