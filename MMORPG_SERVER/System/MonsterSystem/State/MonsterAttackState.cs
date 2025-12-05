using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.FightSystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.MonsterSystem.State
{
    public class MonsterAttackState : IState
    {
        private MonsterAi _monsterAi;

        private float _timer;

        private float _attackInterval = 0.7f;

        public MonsterAttackState(MonsterAi monsterAi)
        {
            _monsterAi = monsterAi;
        }

        public bool Condition(IState newState)
        {
            return true;
        }

        public async void Enter()
        {
            _monsterAi._monster._stateId = (int)MonsterState.attack;
            await AttackTarget();
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
            if(_timer > _attackInterval)
            {
                _monsterAi.ChangeState(MonsterState.idle);
                _timer = 0;
            }
        }

        private async Task AttackTarget()
        {
            var attacker = _monsterAi._monster;
            var target = _monsterAi._chaseTarget;

            bool isHit = await EntityManager.Instance.IsAttackTargetVaild(attacker, target, 3.5f, 500);
            //命中
            if (isHit)
            {
                var demage = FightManager.Instance.GetFightHurtValue(attacker, target);
                PlayerManager.Instance.Broadcast(new PlayerAttackResponse()
                {
                    IsSuccessfulAttack = true,
                    CamboCount = 0,
                    Damage = demage,
                    IsHit = true,
                    PlayerId = attacker._entityId,
                    TargetId = target._entityId
                }, attacker);
            }
        }
    }
}
