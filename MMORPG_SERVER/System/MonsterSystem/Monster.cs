using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.Tool;
using System.Net;
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

        public List<Vector3> _movePosition; //巡逻点

        public int _hp;

        public Monster
            (int entityId, 
            EntityType entityType, 
            UnitDefine unitDefine, 
            Vector3 pos, 
            float dir,
            MonsterAi controller,
            List<Vector3> movePos,
            int hp) : base(entityId, entityType, unitDefine, pos, dir)
        {
            _controller = controller;
            _movePosition = movePos;
            _hp = hp;
        }
    }
}
