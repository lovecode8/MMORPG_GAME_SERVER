using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.AttributeSystem
{
    //属性管理器
    public class AttributeManager : Singleton<AttributeManager>
    {
        private AttributeManager() { }

        //玩家id对应属性
        private Dictionary<int, Attribute> _attributeDictionary = new();

        public void AddPlayer(Player player)
        {
            int userId = player._user._userId;

            //TODO:去数据库获取玩家的装备信息
            _attributeDictionary.Add(player._playerId, new Attribute()
            {
                _atkAddition = 0,
                _defAddition = 0
            });
        }
    }
}
