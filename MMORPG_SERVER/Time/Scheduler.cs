using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Time
{
    //实现循环执行的任务
    public class Scheduler : Singleton<Scheduler>
    {
        public TimeWheel _timeWheel;

        private Scheduler()
        {
            _timeWheel = new();

            Task.Run(() =>
            {
                _ = _timeWheel.Start();
            });
        }

        public void AddScheduler(int delayMs, Action task)
        {
            _timeWheel.AddTask(delayMs, (t) =>
            {
                task.Invoke();
                _timeWheel.AddTask(delayMs, t.action);
            });
        }
    }
}
