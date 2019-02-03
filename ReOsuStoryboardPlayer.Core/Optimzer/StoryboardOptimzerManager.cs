using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Optimzer
{
    public static class StoryboardOptimzerManager
    {
        static HashSet<OptimzerBase> optimzers=new HashSet<OptimzerBase>();

        public static void AddOptimzer<T>() where T : OptimzerBase, new()
        {
            if (optimzers.OfType<T>().Any())
                return;

            AddOptimzer(new T());
        }

        public static void AddOptimzer(OptimzerBase optimzer)
        {
            if (optimzer==null)
                return;

            optimzers.Add(optimzer);

            Log.User($"Add optimzer : {optimzer.GetType().Name}");
        }

        public static void RemoveOptimzer(OptimzerBase optimzer)
        {
            if (optimzer==null)
                return;

            optimzers.Remove(optimzer);

            Log.User($"Remove optimzer : {optimzer.GetType().Name}");
        }

        public static void RemoveOptimzer<T>() where T:OptimzerBase
        {
            var type = typeof(T);

            RemoveOptimzer(optimzers.FirstOrDefault(x => type.IsInstanceOfType(x)));
        }

        public static void Optimze(int level,IEnumerable<StoryboardObject> objects)
        {
            foreach (var opzimer in optimzers)
            {
                opzimer.Optimze(level, objects);
            }
        }
    }
}
