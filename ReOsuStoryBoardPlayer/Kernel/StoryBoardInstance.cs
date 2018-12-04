using OpenTK.Graphics.OpenGL;
using ReOsuStoryBoardPlayer.Base;
using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Player;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReOsuStoryBoardPlayer
{
    /// <summary>
    /// SB更新物件的核心，通过Update来更新物件
    /// </summary>
    public class StoryBoardInstance
    {
        /// <summary>
        /// 已加载的物件集合
        /// </summary>
        public LinkedList<StoryBoardObject> StoryboardObjectList { get; private set; }

        private LinkedListNode<StoryBoardObject> CurrentScanNode;

        /// <summary>
        /// 正在执行的物件集合
        /// </summary>
        public Dictionary<Layout, List<StoryBoardObject>> UpdatingStoryboardObjects { get; private set; } = new Dictionary<Layout, List<StoryBoardObject>>();
        
        public static StoryBoardInstance Instance { get;private set; }

        public BeatmapFolderInfo Info { get; }

        public StoryBoardInstance(BeatmapFolderInfo info)
        {
            Instance=this;

            Info=info;

            StoryboardObjectList = new LinkedList<StoryBoardObject>();

            //int audioLeadIn = 0;
            
            #region Load and Parse osb/osu file

            using (StopwatchRun.Count("Load and Parse osb/osu file"))
            {
                List<StoryBoardObject> temp_objs_list = new List<StoryBoardObject>(), parse_osb_storyboard_objs = new List<StoryBoardObject>();

                //get objs from osu file
                List<StoryBoardObject> parse_osu_storyboard_objs = StoryboardParserHelper.GetStoryBoardObjects(info.osu_file_path);
                AdjustZ(parse_osu_storyboard_objs, 0);

                if ((!string.IsNullOrWhiteSpace(info.osb_file_path)) && File.Exists(info.osb_file_path))
                {
                    parse_osb_storyboard_objs = StoryboardParserHelper.GetStoryBoardObjects(info.osb_file_path);
                    AdjustZ(parse_osb_storyboard_objs, parse_osu_storyboard_objs?.Count()??0);
                }


                temp_objs_list = CombineStoryBoardObjects(parse_osb_storyboard_objs, parse_osu_storyboard_objs);

                //delete Background object if there is a normal storyboard object which is same image file.
                var background_obj = temp_objs_list.Where(c => c is StoryboardBackgroundObject).FirstOrDefault();
                if (temp_objs_list.Any(c => c.ImageFilePath == background_obj?.ImageFilePath && (!(c is StoryboardBackgroundObject))))
                {
                    Log.User($"Found another same background image object and delete background object.");
                    temp_objs_list.Remove(background_obj);
                }
                else
                {
                    if (background_obj != null)
                        background_obj.Z = -1;
                }

                foreach (var obj in temp_objs_list)
                    StoryboardObjectList.AddLast(obj);

                StoryboardObjectList.AsParallel().ForAll(c => c.SortCommands());
                
                CurrentScanNode=StoryboardObjectList.First;
            }

            #endregion Load and Parse osb/osu file

            #region Create LayoutListMap

            foreach (Layout item in Enum.GetValues(typeof(Layout)))
            {
                UpdatingStoryboardObjects.Add(item, new List<StoryBoardObject>());
            }

            #endregion Create LayoutListMap

            void AdjustZ(List<StoryBoardObject> list, int base_z)
            {
                list.Sort((a, b) => (int)(a.FileLine - b.FileLine));
                for (int i = 0; i < list.Count; i++)
                    list[i].Z = base_z + i;
            }
        }

        private List<StoryBoardObject> CombineStoryBoardObjects(List<StoryBoardObject> osb_list, List<StoryBoardObject> osu_list)
        {
            #region Safe Check

            if (osb_list == null)
            {
                osb_list = new List<StoryBoardObject>();
            }

            if (osu_list == null)
            {
                osu_list = new List<StoryBoardObject>();
            }

            #endregion Safe Check

            List<StoryBoardObject> result = new List<StoryBoardObject>(osb_list);
            result.AddRange(osu_list);

            result.Sort((a, b) =>
            {
                return a.FrameStartTime - b.FrameStartTime;
            });

            return result;
        }

        /// <summary>
        /// 重置物件的时间轴状态
        /// </summary>
        public void Flush()
        {
            foreach (var pair in UpdatingStoryboardObjects)
            {
                pair.Value.Clear();
            }

            CurrentScanNode = StoryboardObjectList.First;

            StoryboardObjectList.AsParallel().ForAll((obj) => obj.markDone = false);
        }
        
        private bool Scan(float current_time)
        {
            LinkedListNode<StoryBoardObject> LastAddNode = null;

            while (CurrentScanNode != null && CurrentScanNode.Value.FrameStartTime <= current_time/* && current_time <= CurrentScanNode.Value.FrameEndTime*/ )
            {
                var obj = CurrentScanNode.Value;
                if (current_time > obj.FrameEndTime)
                {
                    CurrentScanNode = CurrentScanNode.Next;
                    continue;
                }

                obj.markDone = false;
                UpdatingStoryboardObjects[obj.layout].Add(obj);

                LastAddNode = CurrentScanNode;

                CurrentScanNode = CurrentScanNode.Next;
            }

            if (LastAddNode != null)
            {
                CurrentScanNode = LastAddNode.Next;
            }

            return /*isAdd*/LastAddNode != null;
        }

        /// <summary>
        /// 更新物件，如果逆向执行必须先Flush()后Update()
        /// </summary>
        /// <param name="current_time"></param>
        public void Update(float current_time)
        {
            var t = runTimer.ElapsedMilliseconds;

            bool hasAdded = Scan(current_time);

            foreach (var objs in UpdatingStoryboardObjects.Values)
            {
                if (hasAdded)
                {
                    objs.Sort((a, b) =>
                    {
                        return a.Z - b.Z;
                    });
                }

                foreach (var obj in objs)
                {
                    if (current_time < obj.FrameStartTime || current_time > obj.FrameEndTime)
                        obj.markDone = true;
                    else
                        obj.Update(current_time);
                }
            }

            //remove unused objects
            foreach (var objs in UpdatingStoryboardObjects.Values)
            {
                objs.RemoveAll((obj) =>
                {
                    if (!obj.markDone)
                        return false;

                    return true;
                });
            }

            UpdateCastTime = runTimer.ElapsedMilliseconds - t;
        }

        ~StoryBoardInstance()
        {

        }
    }
}