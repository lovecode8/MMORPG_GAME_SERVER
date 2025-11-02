using MMORPG_SERVER.Data.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.EntitySystem
{
    //实体类型
    public enum EntityType
    {
        Player = 0,
        Monster = 1,
        NPC = 2,
        Item = 3
    }

    //场景中的实体
    public class Entity
    {
        public int _entityId;
        public EntityType _entityType;
        public UnitDefine _unitDefine;
        public bool _isValid;
        public Vector3 _position;
        public Vector3 _direction;

        public Entity(int entityId, EntityType entityType, UnitDefine unitDefine, Vector3 pos, Vector3 dir)
        {
            _entityId = entityId;
            _entityType = entityType;
            _unitDefine = unitDefine;
            _position = pos;
            _direction = dir;
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }
}
