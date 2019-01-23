namespace ReOsuStoryBoardPlayer.Base
{
    public enum Event
    {
        Fade = 1,
        Move = 0,
        Scale = 2,
        VectorScale = 3,
        Rotate = 4,
        Color = 5,
        Parameter = 6,
        MoveX = 7,
        MoveY = 8,
        Loop = 9,
        Trigger = 10,

        //custon events
        VerticalFlip = 11,

        HorizonFlip = 12,
        AdditiveBlend = 13,

        Unknown = 2857
    }
}