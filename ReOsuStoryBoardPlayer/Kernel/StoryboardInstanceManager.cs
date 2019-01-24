namespace ReOsuStoryboardPlayer.Kernel
{
    public static class StoryboardInstanceManager
    {
        public static StoryboardInstance ActivityInstance { get; private set; }

        public static void ApplyInstance(StoryboardInstance instance)
        {
            ActivityInstance=instance;
        }
    }
}