using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Optimzer;
using ReOsuStoryboardPlayer.Core.Parser.Collection;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayer.Core.Parser.Stream;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReOsuStoryboardPlayer.Parser
{
    public static class StoryboardParserHelper
    {
        private static bool optimzer_add = false;

        public static List<StoryboardObject> GetStoryboardObjects(string path)
        {
            OsuFileReader reader = new OsuFileReader(path);

            VariableCollection collection = new VariableCollection(new VariableReader(reader).EnumValues());

            EventReader er = new EventReader(reader, collection);

            StoryboardReader StoryboardReader = new StoryboardReader(er);

            List<StoryboardObject> list;

            using (StopwatchRun.Count($"Parse&Optimze Storyboard Objects/Commands from {path}"))
            {
                list=StoryboardReader.EnumValues().ToList();
                list.RemoveAll(c => c==null);

                foreach (var obj in list)
                {
                    obj.CalculateAndApplyBaseFrameTime();
                }

                if (PlayerSetting.StoryboardObjectOptimzeLevel>0)
                {
                    if (!optimzer_add)
                        InitOptimzerManager();

                    StoryboardOptimzerManager.Optimze(PlayerSetting.StoryboardObjectOptimzeLevel,list);
                }
            }

            return list;
        }

        private static void InitOptimzerManager()
        {
            var base_type = typeof(OptimzerBase);

            var need_load_optimzer = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.GetTypes())
                .SelectMany(l => l)
                .Where(x => x.IsClass&&!x.IsAbstract&&x.IsSubclassOf(base_type)).Select(x=> {
                    try
                    {
                        return Activator.CreateInstance(x);
                    }
                    catch (Exception e)
                    {
                        Log.Warn($"Can't load optimzer \"{x.Name}\" :"+e.Message);
                        return null;
                    }
                }).OfType<OptimzerBase>();

            foreach (var optimzer in need_load_optimzer)
                StoryboardOptimzerManager.AddOptimzer(optimzer);

            optimzer_add=true;
        }
    }
}