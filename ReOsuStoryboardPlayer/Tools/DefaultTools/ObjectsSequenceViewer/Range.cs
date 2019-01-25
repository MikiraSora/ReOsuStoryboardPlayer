namespace ReOsuStoryboardPlayer.Tools.DefaultTools.ObjectsSequenceViewer
{
    public struct Range
    {
        public int End;
        public int Start;

        public bool InRange(int cur) => cur>=Start&&cur<=End;
    }
}