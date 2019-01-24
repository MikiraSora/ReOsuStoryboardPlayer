using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryboardPlayer.Core.Utils;
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
        /// 已加载的物件集合
        /// </summary>
        public LinkedList<StoryboardObject> StoryboardObjectList { get; private set; }

        private LinkedListNode<StoryboardObject> CurrentScanNode;

        /// <summary>
        /// 正在执行的物件集合
        /// </summary>
        public List<StoryboardObject> UpdatingStoryboardObjects { get; private set; }

        private ParallelOptions parallel_options = new ParallelOptions() { MaxDegreeOfParallelism=Setting.UpdateThreadCount };

        public StoryboardUpdater(List<StoryboardObject> objects)
        {
            StoryboardObjectList=new LinkedList<StoryboardObject>();

            //int audioLeadIn = 0;
            /*
            using (StopwatchRun.Count("Load and Parse osb/osu file"))
            {
                List<StoryboardObject> temp_objs_list = new List<StoryboardObject>(), parse_osb_Storyboard_objs = new List<StoryboardObject>();

                //get objs from osu file
                List<StoryboardObject> parse_osu_Storyboard_objs = string.IsNullOrWhiteSpace(info.osu_file_path)?new List<StoryboardObject>():StoryboardParserHelper.GetStoryboardObjects(info.osu_file_path);
                AdjustZ(parse_osu_Storyboard_objs, 0);

                if ((!string.IsNullOrWhiteSpace(info.osb_file_path))&&File.Exists(info.osb_file_path))
                {
                    parse_osb_Storyboard_objs=StoryboardParserHelper.GetStoryboardObjects(info.osb_file_path);
                    AdjustZ(parse_osb_Storyboard_objs, 0);
                }

                temp_objs_list=CombineStoryboardObjects(parse_osb_Storyboard_objs, parse_osu_Storyboard_objs);

                //delete Background object if there is a normal Storyboard object which is same image file.
                var background_obj = temp_objs_list.Where(c => c is StoryboardBackgroundObject).FirstOrDefault();
                if (temp_objs_list.Any(c => c.ImageFilePath==background_obj?.ImageFilePath&&(!(c is StoryboardBackgroundObject))))
                {
                    Log.User($"Found another same background image object and delete all background objects.");
                    temp_objs_list.RemoveAll(x=>x is StoryboardBackgroundObject);
                }
                else
                {
                    if (background_obj!=null)
                        background_obj.Z=-1;
                }

                temp_objs_list.Sort((a, b) =>
                {
                    return a.FrameStartTime-b.FrameStartTime;
                });

                foreach (var obj in temp_objs_list)
                    StoryboardObjectList.AddLast(obj);

                StoryboardObjectList.AsParallel().ForAll(c => c.SortCommands());

                CurrentScanNode=StoryboardObjectList.First;
            }
            */

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

            foreach (var obj in objects)
                StoryboardObjectList.AddLast(obj);

            StoryboardObjectList.AsParallel().ForAll(c => c.SortCommands());

            CurrentScanNode=StoryboardObjectList.First;

            var limit_update_count = StoryboardObjectList.CalculateMaxUpdatingObjectsCount();

            UpdatingStoryboardObjects=new List<StoryboardObject>(limit_update_count);
        }

        /// <summary>
        /// 重置物件的时间轴状态
        /// </summary>
        private void Flush()
        {
            UpdatingStoryboardObjects.Clear();

            CurrentScanNode=StoryboardObjectList.First;

            //重置触发器状态
            TriggerListener.DefaultListener.Reset();
        }

        private bool Scan(float current_time)
        {
            LinkedListNode<StoryboardObject> LastAddNode = null;

            while (CurrentScanNode!=null&&CurrentScanNode.Value.FrameStartTime<=current_time/* && current_time <= CurrentScanNode.Value.FrameEndTime*/ )
            {
                var obj = CurrentScanNode.Value;

                if (current_time>obj.FrameEndTime)
                {
                    CurrentScanNode=CurrentScanNode.Next;
                    continue;
                }

                //重置物件变换初始值
                obj.ResetTransform();

                //添加到更新列表
                UpdatingStoryboardObjects.Add(obj);

                LastAddNode=CurrentScanNode;

                CurrentScanNode=CurrentScanNode.Next;
            }

            if (LastAddNode!=null)
            {
                CurrentScanNode=LastAddNode.Next;
            }

            return LastAddNode!=null;
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
                UpdatingStoryboardObjects.RemoveAll((obj) => current_time>obj.FrameEndTime||current_time<obj.FrameStartTime);

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

        ~StoryboardUpdater()
        {
        }

        //public override string ToString() => $"{Info.folder_path}";
    }
}