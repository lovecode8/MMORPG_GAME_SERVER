using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.UserSystem;
using Serilog;
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

        //玩家所在的网格
        public Vector2 _currentCell;

        public User _user;

        public DbCharacter _dbCharacter;

        public Player(
            int entityId,
            EntityType entityType, 
            UnitDefine unitDefine, 
            Vector3 pos, 
            float dir,
            User user,
            DbCharacter character) : base(entityId, entityType, unitDefine, pos, dir)
        {
            _user = user;
            _dbCharacter = character;
            _currentCell = PlayerManager.Instance.GetCellByPosition(pos);
        }

        public void GetHurt(int demage)
        {
            _dbCharacter.Hp -= demage;
            if(_dbCharacter.Hp <= 0)
            {
                //处理死亡逻辑
                Log.Information($"[Player] {_playerId}死亡");
            }
        }
    }
}
