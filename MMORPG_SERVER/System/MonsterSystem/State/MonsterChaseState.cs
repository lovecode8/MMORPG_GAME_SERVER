using MMORPG_SERVER.Extension;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.MonsterSystem.State
{
    public class MonsterChaseState : IState
    {
        private MonsterAi _monsterAi;

        private float _chaseSpeed = 4f;

        private float _chaseMaxDistance = 12f;

        private float _attackDistance = 3f;

        public MonsterChaseState(MonsterAi monsterAi)
        {
            _monsterAi = monsterAi;
        }

        public bool Condition(IState newState)
        {
            return true;
        }

        public void Enter()
        {
            Log.Information("Chase");
            _monsterAi._monster._stateId = (int)MonsterState.chase;
        }

        public void Exit()
        {
            
        }

        public void FixedUpdate()
        {
            
        }

        public void Update()
        {
            ChaseTarget();
        }

        private void ChaseTarget()
        {
            var distance = Vector3.Distance(_monsterAi._monster._position, _monsterAi._chaseTarget._position);
            if(distance < _attackDistance && _monsterAi._canAttack)
            {
                _monsterAi.ChangeState(MonsterState.attack);
                _ = _monsterAi.LockAttack();
            }
            else if(distance > _chaseMaxDistance)
            {
                _monsterAi.ChangeState(MonsterState.move);
            }

            if (distance > _attackDistance && distance < _chaseMaxDistance)
            {
                //pos
                var direction = _monsterAi._chaseTarget._position - _monsterAi._monster._position;
                direction.Y = 0;
                var moveDelta = Vector3.Normalize(direction) * _chaseSpeed * MMORPG_SERVER.Time.Timer.deltaTime;
                _monsterAi._monster._position += moveDelta;

                //rot
                float yawRadians = (float)Math.Atan2(direction.X, direction.Z);
                float yawDeg = yawRadians * (180 / (float)Math.PI);
                _monsterAi._monster._rotationY = yawDeg;
            }
        }
    }
}
