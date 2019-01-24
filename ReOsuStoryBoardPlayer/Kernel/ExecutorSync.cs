using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Kernel
{
    public static class ExecutorSync
    {
        private static Queue<Task> task_list = new Queue<Task>();

        /// <summary>
        /// StoryboardWindow::OnUpdateFrame()同线程执行，该线程将会被堵塞直至被执行
        /// </summary>
        /// <param name="func"></param>
        public static Task PostTask(Action func)
        {
            var task = new Task(func);
            task_list.Enqueue(task);
            return task;
        }

        internal static void ClearTask()
        {
            while (task_list.Count!=0)
            {
                var task = task_list.Dequeue();
                task.RunSynchronously();
            }
        }
    }
}