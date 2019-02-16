using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryboardPlayer.Core.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.Kernel
{
    /// <summary>
    /// SB更新物件的核心，通过Update()来更新物件
    /// </summary>
    public class StoryboardUpdater
    {
        /// <summary>
        /// 已加载的物件集合,会按照FrameStartTime从小到大排列
        /// </summary>
        public List<StoryboardObject> StoryboardObjectList { get; private set; }

        private int current_index = 0;

        private ConcurrentBag<StoryboardObject> need_resort_objects = new ConcurrentBag<StoryboardObject>();

        /// <summary>
        /// 正在执行的物件集合,会按照渲染顺序Z排列
        /// </summary>
        public List<StoryboardObject> UpdatingStoryboardObjects { get; private set; }

        private readonly ParallelOptions parallel_options = new ParallelOptions() { MaxDegreeOfParallelism=Setting.UpdateThreadCount };

        public StoryboardUpdater(List<StoryboardObject> objects)
        {
            StoryboardObjectList=new List<StoryboardObject>();

            //delete Background object if there is a normal Storyboard object which is same image file.
            var background_obj = objects.Where(c => c is StoryboardBackgroundObject).FirstOrDefault();
            if (objects.Any(c => c.ImageFilePath==background_obj?.ImageFilePath&&(!(c is StoryboardBackgroundObject))))
            {
                Log.User($"Found another same background image object and delete all background objects.");
                objects.RemoveAll(x => x is StoryboardBackgroundObject);
            }
            else
            {
                if (background_obj!=null)
                    background_obj.Z=-1;
            }

            objects.Sort((a, b) =>
            {
                return a.FrameStartTime-b.FrameStartTime;
            });

            StoryboardObjectList=objects;

            var limit_update_count = StoryboardObjectList.CalculateMaxUpdatingObjectsCount();

            UpdatingStoryboardObjects=new List<StoryboardObject>(limit_update_count);

            Flush();
        }

        /// <summary>
        /// 重置物件的时间轴状态
        /// </summary>
        private void Flush()
        {
            UpdatingStoryboardObjects.Clear();

            current_index=0;

            //重置触发器状态
            TriggerListener.DefaultListener.Reset();
        }

        private bool Scan(float current_time)
        {
            bool add = false;

            foreach (var obj in need_resort_objects)
                StoryboardObjectList.Remove(obj);

            while (need_resort_objects.TryTake(out var obj))
            {
                var i = BinarySearchInsertableIndex(obj.FrameStartTime);
                StoryboardObjectList.Insert(i, obj);

                if (obj.FrameStartTime<=current_index)
                    TryAdd(obj);

                //Log.Debug($"Object ({obj}) FrameTime had been changed({obj.FrameStartTime} - {obj.FrameEndTime})");
            }

            while (current_index<StoryboardObjectList.Count)
            {
                var obj = StoryboardObjectList[current_index];

                if (obj.FrameStartTime>current_time)
                    break;

                TryAdd(obj);

                current_index++;
            }

            return add;

            void TryAdd(StoryboardObject obj)
            {
                if (current_time>obj.FrameEndTime)
                    return;

                obj.ResetTransform();
                obj.CurrentUpdater=this;
                UpdatingStoryboardObjects.Add(obj);
                add=true;
            }

            int BinarySearchInsertableIndex(float time)
            {
                int min = 0, max = StoryboardObjectList.Count-2;

                int insert = 0;

                //fast check for appending
                if (time>=StoryboardObjectList.LastOrDefault()?.FrameStartTime)
                    insert=StoryboardObjectList.Count;
                else
                {
                    while (min<=max)
                    {
                        int i = (max+min)/2;

                        var cmd = StoryboardObjectList[i];
                        var next_cmd = StoryboardObjectList[i+1];

                        if (cmd.FrameStartTime<=time&&time<=next_cmd.FrameStartTime)
                            return i+1;

                        if (cmd.FrameStartTime>=time)
                            max=i-1;
                        else
                            min=i+1;
                    }
                }

                return insert;
            }
        }

        private float prev_time = int.MinValue;

        /// <summary>
        /// 更新物件，因为是增量更新维护，所以current_time递减或变小时，必须先Flush()后Update().
        /// </summary>
        /// <param name="current_time"></param>
        public void Update(float current_time)
        {
            if (current_time<prev_time)
                Flush();
            else
                UpdatingStoryboardObjects.RemoveAll((obj) => (current_time>obj.FrameEndTime||current_time<obj.FrameStartTime)
                &&(obj.CurrentUpdater=null)==null/*clean CurrentUpdater*/);

            prev_time=current_time;

            bool hasAdded = Scan(current_time);

            if (hasAdded)
            {
                UpdatingStoryboardObjects.Sort((a, b) =>
                {
                    return a.Z-b.Z;
                });
            }

            if (UpdatingStoryboardObjects.Count>=Setting.ParallelUpdateObjectsLimitCount&&Setting.ParallelUpdateObjectsLimitCount!=0)
            {
                Parallel.ForEach(UpdatingStoryboardObjects, parallel_options, obj => obj.Update(current_time));
            }
            else
            {
                foreach (var obj in UpdatingStoryboardObjects)
                    obj.Update(current_time);
            }
        }

        internal void AddNeedResortObject(StoryboardObject obj)
        {
        }

        ~StoryboardUpdater()
        {
        }

        //public override string ToString() => $"{Info.folder_path}";
    }
}