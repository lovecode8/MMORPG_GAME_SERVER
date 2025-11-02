using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Time
{
    //延时执行任务类
    public struct TimeTask
    {
        public int delayMs;

        public Action<TimeTask> action;

        public LinkedListNode<TimeTask>? taskNode;

        public TimeTask(int delay, Action<TimeTask> a)
        {
            delayMs = delay;
            action = a;
        }
    }

    //时间轮类
    public class TimeWheel
    {
        //时间轮大小
        public const int count = 64;

        //执行时间间隔
        public int timeDelay;

        //方法链表
        public readonly LinkedList<TimeTask>[] slot;

        //当前执行到的方法
        public int currentIndex = 0;

        //锁
        public readonly object slotLock = new object();

        public bool isStoped = false;

        public TimeWheel(int delay = 10)
        {
            timeDelay = delay;
            slot = new LinkedList<TimeTask>[count];

            for (int i = 0; i < count; i++)
            {
                slot[i] = new LinkedList<TimeTask>();
            }
        }

        public async Task Start()
        {
            isStoped = false;

            while (!isStoped)
            {
                await Task.Delay(timeDelay);
                Tick();
            }

        }

        public void Tick()
        {
            var tasks = slot[currentIndex];

            if (tasks.Count > 0)
            {
                List<TimeTask> tasks_temp = new List<TimeTask>(tasks);
                tasks.Clear();

                foreach (TimeTask task in tasks_temp)
                {
                    task.action(task);
                }
            }

            currentIndex = (currentIndex + 1) % count;
        }

        public void AddTask(int delayTime, Action<TimeTask> action)
        {
            if (delayTime < timeDelay)
            {
                delayTime = timeDelay;
            }

            int slotIndex = (currentIndex + delayTime / timeDelay) % count;
            TimeTask task = new TimeTask(delayTime, action);

            lock (slotLock)
            {
                task.taskNode = slot[slotIndex].AddLast(task);
            }
        }

        public void RemoveTask(TimeTask task)
        {
            lock (slotLock)
            {
                task.taskNode?.List?.Remove(task.taskNode);
            }
        }

        public void Stop()
        {
            isStoped = true;
        }
    }
}
