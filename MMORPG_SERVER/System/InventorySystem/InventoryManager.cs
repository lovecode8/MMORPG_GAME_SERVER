using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Tool;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.InventorySystem
{
    /// <summary>
    /// 物品类型
    /// </summary>
    public enum ItemType
    {
        Equip, //装备
        Consunable, //消耗品
    }

    /// <summary>
    /// 装备类型
    /// </summary>
    public enum EquipType
    {
        Sword, //刀剑
        Armor, //盔甲
        Addition //加成装备（移速鞋等）
    }

    /// <summary>
    /// 消耗品类型
    /// </summary>
    public enum ConsumableType
    {
        Hp,
        Mp,
        MaxHp,
        MaxMp
    }

    public class InventoryManager : Singleton<InventoryManager>
    {
        private InventoryManager() { }

        //玩家id对应库存列表
        private Dictionary<int, List<InventoryInfo>> _playerInventoryDictionary = new();

        public void Start()
        {
            LoadInventoryFromDatabase();
        }

        //从数据库获取所有玩家的库存列表
        private void LoadInventoryFromDatabase()
        {
            var list = MysqlManager.Instance._freeSql.Select<DbInventory>().ToList();
            foreach (var item in list)
            {
                var inventoryInfo = new InventoryInfo()
                {
                    ItemId = item.itemId,
                    ItemCount = item.itemCount
                };

                if (_playerInventoryDictionary.TryGetValue(item.ownerId, out var itemList))
                {
                    itemList.Add(inventoryInfo);
                }
                else
                {
                    _playerInventoryDictionary.Add(item.ownerId, new());
                    _playerInventoryDictionary[item.ownerId].Add(inventoryInfo);
                }
            }
        }

        //获取单个玩家的库存信息
        public List<InventoryInfo>? GetPlayerInventory(int userId)
        {
            lock (_playerInventoryDictionary)
            {
                if (_playerInventoryDictionary.TryGetValue(userId, out var inventory))
                {
                    return inventory;
                }
                return null;
            }
        }

        public List<InventoryInfo>? UseItem(int userId, int itemId)
        {
            if(_playerInventoryDictionary.TryGetValue(userId, out var inventory))
            {
                foreach(var item in inventory)
                {
                    if(item.ItemId == itemId)
                    {
                        item.ItemCount--;
                        if(item.ItemCount == 0)
                        {
                            inventory.Remove(item);
                        }
                        return inventory;
                    }
                }
            }
            return null;
        }
    }
}
