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
                CreateMonster();
                _timer = 0;
            }
        }

        private void CreateMonster()
        {
            Log.Information($"[MonsterManager] 生成Monster");
            var unitDefine = DataManager.Instance.GetUnitDefine(6);
            var monsterAi = new MonsterAi();

            var monster = new Monster(
                EntityManager.Instance.NewEntityId(),
                EntityType.Monster,
                unitDefine,
                unitDefine.OriginalPosition.ToVector3(),
                0,
                monsterAi
                );
            monsterAi.SetMonster(monster);
            AddMonster(monster);
            EntityManager.Instance.AddEntity(monster);
            MapManager.Instance.EntityEnter(monster);
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
                _monsterCount < 5 &&
                PlayerManager.Instance.GetPlayerCount() > 0;
        }
    }
}
