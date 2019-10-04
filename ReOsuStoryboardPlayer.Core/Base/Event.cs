namespace ReOsuStoryboardPlayer.Core.Base
{
    public enum Event
    {
        Move = 0,
        Fade = 1,
        Scale = 2,
        Rotate = 3,
        Color = 4,

        //internal event.
        VerticalFlip = 5,
        HorizonFlip = 6,
        AdditiveBlend = 7,

        MoveX = 8,
        MoveY = 9,
        VectorScale = 10,
        Loop = 11,
        Trigger = 12,

        Parameter = 9418,
        Unknown = 2857
    }
}