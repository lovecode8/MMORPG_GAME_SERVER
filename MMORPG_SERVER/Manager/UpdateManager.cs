using MMORPG_SERVER.Database;
using MMORPG_SERVER.System.AStarSystem;
using MMORPG_SERVER.System.AttributeSystem;
using MMORPG_SERVER.System.ChatSystem;
using MMORPG_SERVER.System.EffectSystem;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.FriendSystem;
using MMORPG_SERVER.System.GuildSystem;
using MMORPG_SERVER.System.InventorySystem;
using MMORPG_SERVER.System.MissileSystem;
using MMORPG_SERVER.System.MonsterSystem;
using MMORPG_SERVER.System.NpcSystem;
using MMORPG_SERVER.System.ShopSystem;
using MMORPG_SERVER.System.SkillSystem;
using MMORPG_SERVER.System.TaskSystem;
using MMORPG_SERVER.System.UserSystem;
using MMORPG_SERVER.Time;
using MMORPG_SERVER.Tool;
using Serilog;
using Timer = MMORPG_SERVER.Time.Timer;

namespace MMORPG_SERVER.Manager
{
    //服务器更新管理器
    public class UpdateManager : Singleton<UpdateManager>
    {
        public Queue<Action> _taskQueue = new();

        public Queue<Action> _tempTaskQueue = new();

        public int _updateTime = 100;

        private UpdateManager() { }

        public async Task Start()
        {
            Log.Information("[UpdateManager]连接数据库中......");
            MysqlManager.Instance.Start();

            //各管理器负责初始化
            DataManager.Instance.Start();
            Log.Information("[DataManager] 初始化完成");

            UserManager.Instance.Start();
            Log.Information("[UserManager] 初始化完成");

            FriendManager.Instance.Start();
            Log.Information("[FriendManager] 初始化完成");

            ChatManager.Instance.Start();
            Log.Information("[ChatManager] 初始化完成");

            GuildManager.Instance.Start();
            Log.Information("[GuildManager] 初始化完成");

            InventoryManager.Instance.Start();
            Log.Information("[InventoryManager] 初始化完成");

            AttributeManager.Instance.Start();
            Log.Information("[AttributeManager] 初始化完成");

            MonsterManager.Instance.Start();
            Log.Information("[MonsterManager] 初始化完成");

            AStarManager.Instance.Start();
            Log.Information("[AStarManager] 初始化完成");

            SkillManager.Instance.Start();
            Log.Information("[SkillManager] 初始化完成");

            ShopManager.Instance.Start();
            Log.Information("[ShopManager] 初始化完成");

            TaskManager.Instance.Start();
            Log.Information("[TaskManager] 初始化完成");

            NpcManager.Instance.Start();
            Log.Information("[NpcManager] 初始化完成");

            Scheduler.Instance.AddScheduler(_updateTime, Update);
        }

        private void Update()
        {
            Timer.Tick();

            lock (_taskQueue)
            {
                (_taskQueue, _tempTaskQueue) = (_tempTaskQueue, _taskQueue);

                foreach (Action task in _tempTaskQueue)
                {
                    try
                    {
                        task.Invoke();
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex.Message);
                    }
                }
                _tempTaskQueue.Clear();
            }

            //各模块处理更新逻辑
            try
            {
                DataManager.Instance.Update();
                EntityManager.Instance.Update();
                MonsterManager.Instance.Update();
                SkillManager.Instance.Update();
                MissileManager.Instance.Update();
                EffectManager.Instance.Update();
                NpcManager.Instance.Update();
            }
            catch (Exception ex)
            {
                Log.Error($"[UpdateManager] 执行update出错 {ex.Message}");
            }

        }

        public void AddTask(Action task)
        {
            lock (_taskQueue)
            {
                _taskQueue.Enqueue(task);
            }
        }
    }
}
