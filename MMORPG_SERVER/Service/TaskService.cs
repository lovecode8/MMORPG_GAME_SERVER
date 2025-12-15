using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.TaskSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Service
{
    //处理任务相关消息
    public class TaskService : ServiceBase<TaskService>
    {
        //处理加载任务请求
        public void OnHandle(object sender, LoadTaskRequest loadTaskRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                int userId = channel._user._userId;
                var taskList = TaskManager.Instance.GetTaskListByUserId(userId);
                var response = new LoadTaskResponse();
                if(taskList == null)
                {
                    channel.SendAsync(response);
                }
                else
                {
                    response.TaskList.AddRange(taskList);
                    channel.SendAsync(response);
                }
            });
        }
    }
}
