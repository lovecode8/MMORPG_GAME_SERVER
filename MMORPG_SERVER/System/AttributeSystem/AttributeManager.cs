using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.InventorySystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.System.UserSystem;
using MMORPG_SERVER.Tool;
using Serilog;
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

        public void Start()
        {
            LoadAttributeFromDatabase();
        }

        private void LoadAttributeFromDatabase()
        {
            var list = MysqlManager.Instance._freeSql.Select<DbAttribute>().ToList();
            foreach(var attribute in list)
            {
                _attributeDictionary.Add(attribute.ownerId, new()
                {
                    _atkAddition = attribute.atkAddition,
                    _defAddition = attribute.defAddition
                });
            }
        }

        public Attribute? GetPlayerAttribute(int userId)
        {
            lock (_attributeDictionary)
            {
                if (_attributeDictionary.TryGetValue(userId, out var attribute))
                {
                    return attribute;
                }
                return null;
            }
        }

        //获取对应物品的加成属性，并同步在服务端
        public int GetConsumableValue(Player player, ItemDefine item)
        {
            switch ((ConsumableType)item.ConsumableType)
            {
                case ConsumableType.Hp:
                    player._dbCharacter.Hp = Math.Clamp
                        (player._dbCharacter.Hp + item.Hp, 
                        0, 
                        DataManager.Instance.GetUnitDefine
                        (player._dbCharacter.UnitId).Hp + player._dbCharacter.MaxHpAddition);
                    return item.Hp;

                case ConsumableType.Mp:
                    player._dbCharacter.Mp = Math.Clamp
                        (player._dbCharacter.Mp + item.Mp,
                        0,
                        DataManager.Instance.GetUnitDefine
                        (player._dbCharacter.UnitId).Mp + player._dbCharacter.MaxMpAddition);
                    return item.Mp;

                case ConsumableType.MaxHp:
                    player._dbCharacter.MaxHpAddition += item.MaxHp;
                    return item.MaxHp;

                case ConsumableType.MaxMp:
                    player._dbCharacter.MaxMpAddition += item.MaxMp;
                    return item.MaxMp;
            }

            return 0;
        }

        public void OnUseEquip(int userId, ItemDefine itemDefine)
        {
            lock (_attributeDictionary)
            {
                Attribute attribute = null;
                if (_attributeDictionary.ContainsKey(userId))
                {
                    attribute = _attributeDictionary[userId];
                }
                else
                {
                    _attributeDictionary.Add(userId, new());
                    attribute = _attributeDictionary[userId];

                    MysqlManager.Instance._freeSql.Insert<DbAttribute>(new DbAttribute()
                    {
                        ownerId = userId
                    }).ExecuteAffrows();
                }

                switch ((EquipType)itemDefine.EquipType)
                {
                    case EquipType.Sword:
                        attribute._atkAddition += itemDefine.Atk;

                        MysqlManager.Instance._freeSql.Update<DbAttribute>().
                            Where(a => a.ownerId == userId).
                            Set(a => a.atkAddition, attribute._atkAddition)
                            .ExecuteAffrows();

                        Log.Information($"[AttributeManager] 使用装备成功：增加atk{itemDefine.Atk}");
                        break;
                    case EquipType.Armor:
                        attribute._defAddition += itemDefine.Def;
                        
                        MysqlManager.Instance._freeSql.Update<DbAttribute>().
                            Where(a => a.ownerId == userId).
                            Set(a => a.defAddition, attribute._defAddition)
                            .ExecuteAffrows();

                        Log.Information($"[AttributeManager] 使用装备成功：增加def{itemDefine.Def}");
                        break;
                }
            }
        }

        public void OnRemoveEquip(int userId, int itemId)
        {
            lock (_attributeDictionary)
            {
                var itemDefine = DataManager.Instance.GetItemDefine(itemId);
                if (_attributeDictionary.TryGetValue(userId, out var attribute))
                {
                    switch ((EquipType)itemDefine.EquipType)
                    {
                        case EquipType.Sword:
                            attribute._atkAddition -= itemDefine.Atk;

                            MysqlManager.Instance._freeSql.Update<DbAttribute>().
                            Where(a => a.ownerId == userId).
                            Set(a => a.atkAddition, attribute._atkAddition)
                            .ExecuteAffrows();

                            Log.Information($"[AttributeManager] 卸下装备成功：减少atk{itemDefine.Atk}");
                            break;
                        case EquipType.Armor:
                            attribute._defAddition -= itemDefine.Def;

                            MysqlManager.Instance._freeSql.Update<DbAttribute>().
                            Where(a => a.ownerId == userId).
                            Set(a => a.defAddition, attribute._defAddition)
                            .ExecuteAffrows();

                            Log.Information($"[AttributeManager] 卸下装备成功：减少def{itemDefine.Def}");
                            break;
                    }
                }
            }
        }
    }
}
