using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.NpcSystem.State
{
    //Npc待机状态
    public class NpcIdleState : IState
    {
        private NpcAi _npcAi;

        private float _idleInterval = 3f;

        private float _timer;

        public NpcIdleState(NpcAi npcAi)
        {
            _npcAi = npcAi;
        }

        public bool Condition(IState newState)
        {
            if(newState is NpcMoveState)
            {
                return _npcAi._userId == -1;
            }
            return true;
        }

        public void Enter()
        {
            _npcAi._npc._stateId = (int)NpcState.idle;
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

            if(_timer > _idleInterval)
            {
                _npcAi.ChangeState(NpcState.move);
                _timer = 0;
            }
        }
    }
}
