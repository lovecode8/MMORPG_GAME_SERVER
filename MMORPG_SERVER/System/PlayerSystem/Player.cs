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

        //是否处于无敌状态--不被追逐、不被技能检测、伤害
        public bool _isInvulnerable;

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

        //受伤
        public void GetHurt(int demage)
        {
            _dbCharacter.Hp -= demage;
            if(_dbCharacter.Hp <= 0)
            {
                //处理死亡逻辑
                Log.Information($"[Player] {_playerId}死亡");
            }
        }

        //增加经验值
        public void AddExp(int exp)
        {
            _dbCharacter.Exp += exp;
            var response = new AddExpResponse() { IsAddLevel = false };

            //升级
            if(_dbCharacter.Exp >= _unitDefine.RequireExp[_dbCharacter.Level])
            {
                _dbCharacter.Exp = _dbCharacter.Exp % _unitDefine.RequireExp[_dbCharacter.Level];
                _dbCharacter.Level++;
                response.IsAddLevel = true;
                response.Level = _dbCharacter.Level;
            }

            response.Exp = _dbCharacter.Exp;
            _user._netChannel.SendAsync(response);
        }

        public void AddInteractedNpc(int unitId)
        {
            _dbCharacter.InteractedUnitId = $"{_dbCharacter.InteractedUnitId}|{unitId}";
        }

        public bool IsInteractedWithNpc(int unitId)
        {
            if (_dbCharacter.InteractedUnitId == "") return false;
            foreach(var id in _dbCharacter.InteractedUnitId.Split('|'))
            {
                if(int.TryParse(id, out var npcId) && npcId == unitId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
