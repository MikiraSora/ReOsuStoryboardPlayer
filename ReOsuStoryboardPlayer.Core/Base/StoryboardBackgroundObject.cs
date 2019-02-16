using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Parser.CommandParser;
using ReOsuStoryboardPlayer.Core.Serialization;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Base
{
    public class StoryboardBackgroundObject : StoryboardObject
    {
        private bool trick_init = false;

        public StoryboardBackgroundObject()
        {
            Z=int.MaxValue;

            AddCommand(new FadeCommand()
            {
                Easing=EasingTypes.None,
                StartTime=-2857,
                EndTime=-2857,
                StartValue=1,
                EndValue=1
            });

            AddCommand(new FadeCommand()
            {
                Easing=EasingTypes.None,
                StartTime=int.MaxValue-2857,
                EndTime=int.MaxValue-2857,
                StartValue=1,
                EndValue=1
            });
        }

        public override void Update(float current_time)
        {
            //if it hasn't call AdjustScale() for adjusting scale and it be will always hiden.
            if (!trick_init)
            {
                Color.W=0;
                return;
            }

            base.Update(current_time);
        }

        public void AdjustScale(int height)
        {
            if (trick_init)
                return;

            trick_init=true;

            float scale = 480.0f/height;

            var scale_commands = CommandParserIntance<ScaleCommand>.Instance.Parse($" S,0,{FrameStartTime},{FrameEndTime},{scale}".Split(','));

            foreach (var cmd in scale_commands)
                AddCommand(cmd);
        }

        public override void OnSerialize(BinaryWriter stream, StringCacheTable cache)
        {
            base.OnSerialize(stream, cache);
            trick_init.OnSerialize(stream);
        }

        public override void OnDeserialize(BinaryReader stream, StringCacheTable cache)
        {
            base.OnDeserialize(stream, cache);
            trick_init.OnDeserialize(stream);
        }

        public override bool Equals(StoryboardObject other)
        {
            return base.Equals(other)&&other is StoryboardBackgroundObject;
        }
    }
}