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

        public static bool EnableSplitMoveScaleCommand { get; set; } = true;

        public static bool EnableLoopCommandExpand { get; set; } = false;

        public static bool FunReverseEasing { get; set; } = false;

        /// <summary>
        /// Update线程数量
        /// </summary>
        public static int UpdateThreadCount { get; set; } = 1;

        public static bool ShowProfileSuggest { get; set; } = false;

        public static bool DebugMode { get; set; } = false;

        public static string UserSkinPath { get; set; } = string.Empty;
    }
}