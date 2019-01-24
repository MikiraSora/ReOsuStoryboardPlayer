﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ReOsuStoryBoardPlayer
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
        /// 最小化
        /// </summary>
        public static bool MiniMode { get; set; } = false;

        public static bool EnableSplitMoveScaleCommand { get; set; } = true;

        public static bool EnableRuntimeOptimzeObjects { get; set; } = true;

        public static bool EnableLoopCommandExpand { get; set; } = false;

        public static bool FunReverseEasing { get; set; } = false;

        public static bool EnableBorderless { get; set; } = false;

        public static bool EnableFullScreen { get; set; } = false;

        public static int Width { get; set; } = 854;

        public static int Height { get; set; } = 480;

        /// <summary>
        /// 一次渲染同贴图同Blend的物件数量
        /// </summary>
        public static int DrawCallInstanceCountMax { get; set; } = 1000;

        /// <summary>
        /// 支持时间插值，但对于低帧率会有延迟出现
        /// </summary>
        public static bool EnableTimestamp { get; set; } = false;

        /// <summary>
        /// Update线程数量
        /// </summary>
        public static int UpdateThreadCount { get; set; } = 1;

        public static int MaxFPS { get; set; } = 0;

        public static bool EnableHighPrecisionFPSLimit { get; set; } = false;

        public static int SsaaLevel { get; set; } = 0;

        public static bool ShowProfileSuggest { get; set; } = false;

        public static bool DebugMode { get; set; } = false;

        public static bool EncodingEnvironment { get; set; } = false;

        public static string UserSkinPath { get; set; } = string.Empty;

        #region Extendsion

        public static void PrintSettings()
        {
            if (MiniMode)
                return;

            var props = typeof(Setting).GetProperties();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=======Setting=======");

            foreach (var prop in props)
            {
                var value = prop.GetValue(null);
                sb.AppendLine($"{prop.Name} = {value}");
            }

            sb.AppendLine("================");

            Log.User($"\n{sb.ToString()}");
        }

        /// <summary>
        /// 配置文件仅仅只读
        /// </summary>
        private const string config_file = @"./player_config.ini";

        internal static void Init()
        {
            try
            {
                if (!File.Exists(config_file))
                    CreateConfigFile();

                HashSet<PropertyInfo> read_prop=new HashSet<PropertyInfo>();

                //你以为我会用win32那坨玩意吗，想太多了.jpg
                var props = typeof(Setting).GetProperties().Where(p=> p.GetSetMethod().IsPublic&&p.GetGetMethod().IsPublic);
                var lines = File.ReadAllLines(config_file);

                foreach (var line in lines.Where(l => l.Contains("=")))
                {
                    var data = line.Split('=');

                    if (data.Length!=2)
                        continue;

                    var name = data[0];
                    var value = data[1]??string.Empty;

                    var prop = props.FirstOrDefault(p => p.Name==name);

                    read_prop.Add(prop);

                    if (prop!=null)
                    {
                        switch (prop.PropertyType.Name.ToLower())
                        {
                            case "boolean":
                                prop.SetValue(null, Convert.ToBoolean(value));
                                break;
                            case "int32":
                                prop.SetValue(null, Convert.ToInt32(value));
                                break;
                            case "single":
                                prop.SetValue(null, Convert.ToSingle(value));
                                break;
                            case "double":
                                prop.SetValue(null, Convert.ToDouble(value));
                                break;
                            case "string":
                                prop.SetValue(null, value.ToString());
                                break;
                            default:
                                break;
                        }
                    }

                    Log.Debug($"set {prop.Name} = {value} from config.ini");
                }

                using (var writer=File.AppendText(config_file))
                {
                    foreach (var prop in props.Except(read_prop))
                        writer.WriteLine($"{prop.Name}={prop.GetValue(null)}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Load config.ini failed! {e.Message}");
            }
        }

        private static void CreateConfigFile()
        {
            using (var writer=new StreamWriter(File.OpenWrite(config_file)))
            {
                writer.WriteLine("[Setting]");
                var props = typeof(Setting).GetProperties();

                foreach (var prop in props)
                {
                    var value = prop.GetValue(null);
                    writer.WriteLine($"{prop.Name}={value}");
                }
            }
        }

        #endregion
    }
}