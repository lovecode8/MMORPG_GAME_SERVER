using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.Time;
using MMORPG_SERVER.Tool;
using Serilog;

namespace MMORPG_SERVER.System.MonsterSystem.State
{
    public class MonsterIdleState : IState
    {
        private MonsterAi _monsterAi;

        private float _timer;

        private float _idleInterval = 1.5f;

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
            _timer += MMORPG_SERVER.Time.Timer.deltaTime;

            //TODO：判断追逐
            var player = PlayerManager.Instance.GetChaseablePlayer(_monsterAi._monster);
            if (player != null)
            {
                _monsterAi.SetChaseTarget(player);
                _monsterAi.ChangeState(MonsterState.chase);
            }

            if(_timer > _idleInterval)
            {
                _monsterAi.ChangeState(MonsterState.move);
                _timer = 0;
            }
        }
    }
}
