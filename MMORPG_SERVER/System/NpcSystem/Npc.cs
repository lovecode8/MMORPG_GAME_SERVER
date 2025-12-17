using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.System.EntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.NpcSystem
{
    public class Npc : Entity
    {

        public List<Vector3> _movePosList = new();

        public NpcAi _npcAi;

        public void SetController(NpcAi npcAi)
        {
            _npcAi = npcAi;
        }

        public void Start()
        {
            _npcAi.Start();
        }

        public void Update()
        {
            _npcAi.Update();
        }

        public Npc(int entityId,
            EntityType entityType,
            UnitDefine unitDefine,
            Vector3 pos,
            float dir,
            List<Vector3> movePos) : base(entityId, entityType, unitDefine, pos, dir)
        {
            _movePosList = movePos;
        }
    }
}
