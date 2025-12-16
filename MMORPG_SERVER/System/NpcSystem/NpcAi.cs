using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.NpcSystem.State;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.NpcSystem
{
    public enum NpcState
    {
        idle,
        move,
        moveToPlayer
    }

    public class NpcAi : FSM<NpcState>
    {
        public Npc _npc;

        //当前目标点
        public Vector3 _currentTarget;

        //当前对话对象的用户ID
        public int _userId = -1;

        public NpcAi(Npc npc)
        {
            _npc = npc;
        }

        public void Start()
        {
            AddState(NpcState.idle, new NpcIdleState(this));
            AddState(NpcState.move, new NpcMoveState(this, _npc._movePosList));
            AddState(NpcState.moveToPlayer, new NpcMoveToPlayerState(this));
            ChangeState(NpcState.idle);
        }

        public async Task WaitNpcWalkToPlayer(int userId, Vector3 pos)
        {
            _userId = userId;
            _currentTarget = pos;
            ChangeState(NpcState.moveToPlayer);
            while(true)
            {
                await Task.Delay(100);
                if(_currentTarget == Vector3.Zero)
                {
                    return;
                }
            }
        }
    }
}
