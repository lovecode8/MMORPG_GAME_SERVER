using FreeSql.DataAnnotations;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Tool;

namespace MMORPG_SERVER.System.TaskSystem
{
    //服务端任务管理器
    public class TaskManager : Singleton<TaskManager>
    {
        private TaskManager() { }

        //玩家任务列表
        private Dictionary<int, List<BaseTask>> _userTaskDict = new();

        public void Start()
        {
            LoadTaskFromDatabase();
        }

        //从数据库加载任务数据
        private void LoadTaskFromDatabase()
        {
            var taskList = MysqlManager.Instance._freeSql.Select<DbTask>().ToList();

            foreach (var task in taskList)
            {
                if(!_userTaskDict.ContainsKey(task.ownerId))
                {
                    _userTaskDict[task.ownerId] = new();
                }

                _userTaskDict[task.ownerId].Add(task.ToBaseTask());
            }

        }

        //获取指定玩家的任务列表
        public List<BaseTask> GetTaskListByUserId(int userId)
        {
            lock(_userTaskDict)
            {
                if(_userTaskDict.TryGetValue(userId, out var list))
                {
                    return list;
                }
                return null;
            }
        }

        //为玩家增加任务
        public void AddTask(int userId, BaseTask task)
        {
            lock (_userTaskDict)
            {
                if (_userTaskDict.TryGetValue(userId, out var list))
                {
                    list.Add(task);
                }
                else
                {
                    _userTaskDict[userId] = new() { task };
                }
            }
        }

        //更新任务进度
        public void UpdateTask(int userId, int taskId, int count)
        {
            lock(_userTaskDict)
            {
                if (_userTaskDict.TryGetValue(userId, out var list))
                {
                    var task = list.Find(t => t.TaskId == taskId);
                    if (task != null)
                    {
                        task.CurrentCount += count;
                        if (task.CurrentCount >= task.TargetCount)
                        {
                            //任务进度完成--移除任务
                            RemoveTask(userId, taskId);
                            //TODO：获得奖励，客户端更新任务
                        }
                    }
                }
            }
        }

        private void RemoveTask(int userId, int taskId)
        {
            lock(_userTaskDict)
            {
                if (_userTaskDict.TryGetValue(userId, out var list))
                {
                    var task = list.Find(t => t.TaskId == taskId);

                    if (task != null)
                    {
                        list.Remove(task);
                    }
                }
            }
        }
    }
}
