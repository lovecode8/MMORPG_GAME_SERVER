using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.MonsterSystem.State
{
    //Monster死亡状态
    public class MonsterDieState : IState
    {
        private MonsterAi _monsterAi;

        private float _deadAnimaitionInterval = 1.5f;

        private float _timer;

        public MonsterDieState(MonsterAi monsterAi)
        {
            _monsterAi = monsterAi;
            _timer = 0;
        }

        public bool Condition(IState newState)
        {
            return false;
        }

        public void Enter()
        {
            _monsterAi._monster._stateId = (int)MonsterState.die;
            _timer = 0;
        }

        public void Exit()
        {
           
        }

        public void FixedUpdate()
        {
            
        }

        public void Update()
        {
            _timer += MMORPG_SERVER.Time.Timer.deltaTime;

            if(_timer > _deadAnimaitionInterval)
            {
                //销毁、生成奖励
                //实体离开游戏
                MonsterManager.Instance.RemoveMonster(_monsterAi._monster);
                EntityManager.Instance.RemoveEntity(_monsterAi._monster);
                MapManager.Instance.EntityLeave(_monsterAi._monster);

                //生成奖励
                //在场景中创建物品
                //TODO：后续增加随机物品功能
                int itemId = 2;
                var itemDefine = DataManager.Instance.GetItemDefine(itemId);
                var entity = new Entity(EntityManager.Instance.NewEntityId(),
                    EntityType.Item,
                    DataManager.Instance.GetUnitDefine(itemDefine.UnitId),
                    _monsterAi._monster._position,
                    0);
                entity.itemId = itemId;
                EntityManager.Instance.AddEntity(entity);
                MapManager.Instance.EntityEnter(entity);
            }
        }
    }
}
