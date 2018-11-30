namespace ReOsuStoryBoardPlayer
{
    public static class Setting
    {
        /// <summary>
        /// 对指定以上的数量的命令进行并行解析
        /// </summary>
        public const int ParallelParseCommandLimitCount = 500;

        /// <summary>
        /// 对指定以上的数量的物件进行并行更新
        /// </summary>
        public const int ParallelUpdateObjectsLimitCount = 100;
    }
}