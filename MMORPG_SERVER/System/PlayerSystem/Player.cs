using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.UserSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.PlayerSystem
{
    //玩家类
    public class Player : Entity
    {
        public int _playerId => _entityId;

        public User _user;

        public int _gold;

        public DbCharacter _dbCharacter;

        public Player(
            int entityId,
            EntityType entityType, 
            UnitDefine unitDefine, 
            Vector3 pos, 
            Vector3 dir,
            User user,
            int gold) : base(entityId, entityType, unitDefine, pos, dir)
        {
            _user = user;
            _gold = gold;
        }
    }
}
