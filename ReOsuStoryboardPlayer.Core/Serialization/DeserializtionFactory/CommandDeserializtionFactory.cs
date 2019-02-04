using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using System;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Serialization.DeserializationFactory
{
    public class CommandDeserializtionFactory
    {
        public static Command Create(BinaryReader stream)
        {
            Event e =(Event)stream.ReadInt32();
            var command = CreateCommand(e);

            command.OnDeserialize(stream);

            return command;
        }

        private static Command CreateCommand(Event e)
        {
            switch (e)
            {
                case Event.Fade:
                    return new FadeCommand();
                case Event.Move:
                    return new MoveCommand();
                case Event.Scale:
                    return new ScaleCommand();
                case Event.VectorScale:
                    return new VectorScaleCommand();
                case Event.Rotate:
                    return new RotateCommand();
                case Event.Color:
                    return new ColorCommand();
                case Event.MoveX:
                    return new MoveXCommand();
                case Event.MoveY:
                    return new MoveYCommand();
                case Event.Loop:
                    return new LoopCommand();
                case Event.Trigger:
                    return new TriggerCommand();
                case Event.VerticalFlip:
                    return new VerticalFlipCommand();
                case Event.HorizonFlip:
                    return new HorizonFlipCommand();
                case Event.AdditiveBlend:
                    return new AdditiveBlendCommand();
                default:
                    throw new Exception("Unknown/Unsupport deserialize event:"+e);
            }
        }
    }
}