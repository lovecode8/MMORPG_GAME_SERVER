using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.Tool;
using System.Numerics;

namespace MMORPG_SERVER.System.MonsterSystem
{
    public enum MonsterState
    {
        idle,
        move,
        chase,
        attack,
        getHit,
        die
    }

    public class Monster : Entity
    {
        public int _monsterId => _entityId;

        public MonsterAi _controller;

        public Monster
            (int entityId, 
            EntityType entityType, 
            UnitDefine unitDefine, 
            Vector3 pos, 
            float dir,
            MonsterAi controller) : base(entityId, entityType, unitDefine, pos, dir)
        {
            _controller = controller;
        }
    }
}
