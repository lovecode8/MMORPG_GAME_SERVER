using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.FightSystem;
using MMORPG_SERVER.Tool;
using Serilog;
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

    //敌人类
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

        public void GetHurt(Entity attacker, int demage)
        {
            _hp -= demage;

            if(_hp <= 0)
            {
                Log.Information($"[Monster] {_entityId}死亡");
                _isDead = true;
                FightManager.Instance.OnMonsterDie(attacker, this);
            }
        }
    }
}
