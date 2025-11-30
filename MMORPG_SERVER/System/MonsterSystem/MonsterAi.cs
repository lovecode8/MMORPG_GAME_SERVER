using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MonsterSystem.State;
using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.MonsterSystem
{
    //MonsterAi控制器
    public class MonsterAi : FSM<MonsterState>
    {
        public Monster _monster;

        public Entity _chaseTarget;

        public bool _canAttack = true;

        private int _attackInterval = 1500;

        public void SetMonster(Monster monster) => _monster = monster;

        public void Start()
        {
            AddMonsterState();
            ChangeState(MonsterState.idle);
        }

        private void AddMonsterState()
        {
            AddState(MonsterState.idle, new MonsterIdleState(this));
            AddState(MonsterState.move, new MonsterMoveState(this, _monster._movePosition));
            AddState(MonsterState.chase, new MonsterChaseState(this));
            AddState(MonsterState.attack, new MonsterAttackState(this));
            AddState(MonsterState.getHit, new MonsterGetHitState(this));
        }

        public void SetChaseTarget(Entity entity) => _chaseTarget = entity;

        public async Task LockAttack()
        {
            _canAttack = false;
            await Task.Delay(_attackInterval);
            _canAttack = true;
        }
    }
}
