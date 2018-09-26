using ReOsuStoryBoardPlayer.Parser.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ReOsuStoryBoardPlayer.Parser.Stream.EventReader;

namespace ReOsuStoryBoardPlayer.Parser.Reader
{
    public class StoryboardReader : IReader<StoryBoardObject>
    {
        public bool IsEnd => Reader.EndOfStream;

        public EventReader Reader { get; }

        public StoryboardReader(EventReader reader)
        {
            Reader = reader;
        }

        public IEnumerable<StoryBoardObject> GetValues(int thread_count)
        {
            if (thread_count == 0)
                foreach (var packet in Reader.GetStoryboardPackets())
                {
                    var o = ParsePacket(packet);
                    if (o != null)
                        yield return o;
                }
            else
            {
                Task<StoryBoardObject>[] tasks = new Task<StoryBoardObject>[thread_count];

                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = new Task<StoryBoardObject>(GetPacketAndParse, TaskCreationOptions.PreferFairness);
                    tasks[i].Start();
                }

                //已经读完SB文本并且全部任务执行完毕,才给跳出循环
                while ((!Reader.EndOfStream)&&(!tasks.Any(t=>t.IsCompleted)))
                {
                    var completed_task = tasks.FirstOrDefault(t => t.IsCompleted);

                    if (completed_task == null)
                        continue;

                    var storyboard_obj = completed_task.Result;

                    //流都没读完，赶出去继续跑
                    if (!Reader.EndOfStream)
                        completed_task.Start();

                    if (storyboard_obj != null)
                        yield return storyboard_obj;
                }
            }
        }
        
        private StoryBoardObject GetPacketAndParse()
        {
            //maybe be locked
            var packet=Reader.GetStoryboardPacket();

            return ParsePacket(packet);
        }

        public IEnumerable<StoryBoardObject> GetValues() => GetValues(0);

        private StoryBoardObject ParsePacket(StoryboardPacket packet)
        {
            return new StoryBoardObject();
        }

        #region Packet Parse

        #endregion
    }
}
