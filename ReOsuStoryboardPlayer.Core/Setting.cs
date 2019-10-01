namespace ReOsuStoryboardPlayer
{
    public static class Setting
    {
        /// <summary>
        /// 对指定以上的数量的命令进行并行解析
        /// </summary>
        public static int ParallelParseCommandLimitCount { get; set; } = 500;

        /// <summary>
        /// 对指定以上的数量的物件进行并行更新
        /// </summary>
        public static int ParallelUpdateObjectsLimitCount { get; set; } = 100;

        /// <summary>
        /// 将Scale和Move命令分别拆成VectalScale和MoveX/MoveY命令
        /// </summary>
        public static bool EnableSplitMoveScaleCommand { get; set; } = false;

        public static bool EnableLoopCommandUnrolling { get; set; } = false;

        public static bool FunReverseEasing { get; set; } = false;

        /// <summary>
        /// Update线程数量
        /// </summary>
        public static int UpdateThreadCount { get; set; } = 1;

        public static bool ShowProfileSuggest { get; set; } = false;

        /// <summary>
        /// 调试模式
        /// </summary>
        public static bool DebugMode { get; set; } = false;

        public static bool AllowLog { get; set; } = true;
    }
}