using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.InventorySystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.System.UserSystem;
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
            _attributeDictionary.Add(userId, new Attribute()
            {
                _atkAddition = 0,
                _defAddition = 0
            });
        }

        public Attribute? GetPlayerAttribute(int userId)
        {
            if (_attributeDictionary.TryGetValue(userId, out var attribute))
            {
                return attribute;
            }
            return null;
        }

        //获取对应物品的加成属性，并同步在服务端
        public int GetConsumableValue(Player player, int itemId)
        {
            var item = DataManager.Instance.GetItemDefine(itemId);

            switch ((ConsumableType)item.ConsumableType)
            {
                case ConsumableType.Hp:
                    player._dbCharacter.Hp += item.Hp;
                    return item.Hp;

                case ConsumableType.Mp:
                    player._dbCharacter.Mp += item.Mp;
                    return item.Mp;

                case ConsumableType.MaxHp:
                    _attributeDictionary[player._user._userId]._maxHpAddition += item.MaxHp;
                    return item.MaxHp;

                case ConsumableType.MaxMp:
                    _attributeDictionary[player._user._userId]._maxMpAddition += item.MaxMp;
                    return item.MaxMp;
            }

            return 0;
        }
    }
}
