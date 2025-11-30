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

        private float _timer;

        private float _getHitInterval = 1f;

        public MonsterGetHitState(MonsterAi monsterAi)
        {
            _monsterAi = monsterAi;
        }

        public bool Condition(IState newState)
        {
            return true;
        }

        public void Enter()
        {
            Log.Information("受击");
            _monsterAi._monster._stateId = (int)MonsterState.getHit;
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

            if(_timer > _getHitInterval)
            {
                _monsterAi.ChangeState(MonsterState.chase);
                _timer = 0;
            }
        }
    }
}
