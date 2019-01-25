namespace ReOsuStoryboardPlayer.Tools
{
    public enum UpdatePriority
    {
        /// <summary>
        /// 每帧调用
        /// </summary>
        EveryFrame,

        /// <summary>
        /// 每秒调用
        /// </summary>
        PerSecond,

        None
    }
}