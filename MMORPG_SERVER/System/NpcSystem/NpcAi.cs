using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.NpcSystem.State;
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
        greet
    }

    public class NpcAi : FSM<NpcState>
    {
        public Npc _npc;

        public NpcAi(Npc npc)
        {
            _npc = npc;
        }

        public void Start()
        {
            AddState(NpcState.idle, new NpcIdleState(this));
            AddState(NpcState.move, new NpcMoveState(this, _npc._movePosList));
            ChangeState(NpcState.idle);
        }
    }
}
