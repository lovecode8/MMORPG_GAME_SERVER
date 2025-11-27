using MMORPG_SERVER.Time;
using MMORPG_SERVER.Tool;
using Serilog;

namespace MMORPG_SERVER.System.MonsterSystem.State
{
    public class MonsterIdleState : IState
    {
        private MonsterAi _monsterAi;

        private float _timer;

        private float _idleInterval = 1f;

        public MonsterIdleState(MonsterAi monsterAi)
        {
            _monsterAi = monsterAi;
        }

        public bool Condition(IState newState)
        {
            return true;
        }

        public void Enter()
        {
            _monsterAi._monster._stateId = (int)MonsterState.idle;
        }

        public void Exit()
        {
            
        }

        public void FixedUpdate()
        {
            
        }

        public void Update()
        {
            Log.Information(_timer.ToString());
            _timer += MMORPG_SERVER.Time.Timer.deltaTime;

            //TODO：判断追逐

            if(_timer > _idleInterval)
            {
                _monsterAi.ChangeState(MonsterState.move);
                _timer = 0;
            }
        }
    }
}
