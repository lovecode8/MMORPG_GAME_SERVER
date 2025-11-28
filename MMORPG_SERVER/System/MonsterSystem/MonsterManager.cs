using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.Time;
using MMORPG_SERVER.Tool;
using Serilog;

namespace MMORPG_SERVER.System.MonsterSystem
{
    //场景所有Monster管理器
    public class MonsterManager : Singleton<MonsterManager>
    {
        private MonsterManager() { }

        private Dictionary<int, Monster> _monsterDictionary = new();

        private int _monsterCount;

        private float _timer;

        private float _createMonsterInterval = 10f;

        private Random _random = new Random();

        public void Start()
        {
            foreach(var monster in _monsterDictionary.Values)
            {
                monster._controller.Start();
            }
        }

        public void Update()
        {
            foreach(var monster in _monsterDictionary.Values)
            {
                monster._controller.Update();
            }

            _timer += MMORPG_SERVER.Time.Timer.deltaTime;
            if (CreateMonsterCondition())
            {
                CreateMonster(_random.Next(6, 8));
                _timer = 0;
            }
        }

        private void CreateMonster(int unitId)
        {
            Log.Information($"[MonsterManager] 生成Monster");
            var unitDefine = DataManager.Instance.GetUnitDefine(unitId);
            var monsterAi = new MonsterAi();

            var monster = new Monster(
                EntityManager.Instance.NewEntityId(),
                EntityType.Monster,
                unitDefine,
                unitDefine.OriginalPosition.ToVector3(),
                0,
                monsterAi,
                new()
                );

            //导入移动数据
            foreach(var pos in unitDefine.MovePosition)
            {
                monster._movePosition.Add(new()
                {
                    X = pos[0],
                    Y = pos[1],
                    Z = pos[2]
                });
            }

            monsterAi.SetMonster(monster);
            monsterAi.Start();
            AddMonster(monster);
            EntityManager.Instance.AddEntity(monster);
            MapManager.Instance.EntityEnter(monster);
            _monsterCount++;
        }

        public void AddMonster(Monster monster)
        {
            if (!_monsterDictionary.ContainsKey(monster._monsterId))
            {
                _monsterDictionary.Add(monster._monsterId, monster);
            }
        }

        public void RemoveMonster(Monster monster)
        {
            if (_monsterDictionary.ContainsKey(monster._monsterId))
            {
                _monsterDictionary.Remove(monster._monsterId);
            }
        }
        
        private bool CreateMonsterCondition()
        {
            return _timer > _createMonsterInterval &&
                _monsterCount < 3 &&
                PlayerManager.Instance.GetPlayerCount() > 0;
        }
    }
}
