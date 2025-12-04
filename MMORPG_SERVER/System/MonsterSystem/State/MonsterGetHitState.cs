using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.MonsterSystem.State
{
    public class MonsterGetHitState : IState
    {
        private MonsterAi _monsterAi;

        private float _timer = 0;

        private float _getHitInterval = 1f;

        public MonsterGetHitState(MonsterAi monsterAi)
        {
            _monsterAi = monsterAi;
        }

        public bool Condition(IState newState)
        {
            if (newState is MonsterDieState) return true;
            return _timer > _getHitInterval;
        }

        public void Enter()
        {
            _monsterAi._monster._stateId = (int)MonsterState.getHit;
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
            Log.Information("受击中");
            _timer += MMORPG_SERVER.Time.Timer.deltaTime;

            if(_timer > _getHitInterval)
            {
                _monsterAi.ChangeState(MonsterState.idle);
                _timer = 0;
            }
        }
    }
}
