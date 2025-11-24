using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.System.UserSystem;
using MMORPG_SERVER.Tool;
using Org.BouncyCastle.Asn1.Cmp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        //玩家id对应装备列表
        private Dictionary<int, int[]> _playerEquipDictionary = new();

        public void Start()
        {
            LoadInventoryFromDatabase();
            LoadEquipFromDatabase();
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

        //从数据库获取所有玩家的装备列表
        private void LoadEquipFromDatabase()
        {
            var list = MysqlManager.Instance._freeSql.Select<DbEquip>().ToList();
            foreach(var equip in list)
            {
                int[] equipList = null;
                if (_playerEquipDictionary.ContainsKey(equip.ownerId))
                {
                    equipList = _playerEquipDictionary[equip.ownerId];
                }
                else
                {
                    _playerEquipDictionary.Add(equip.ownerId, new int[6] { -1, -1, -1, -1, -1, -1 });
                    equipList = _playerEquipDictionary[equip.ownerId];
                }
                equipList[equip.gridIndex] = equip.itemId;
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
                else
                {
                    _playerInventoryDictionary.Add(userId, new());
                    return null;
                }
            }
        }

        //获取单个玩家的装备信息
        public int[]? GetPlayerEquip(int userId)
        {
            lock (_playerEquipDictionary)
            {
                if(_playerEquipDictionary.TryGetValue(userId, out var equipList))
                {
                    return equipList;
                }
                else
                {
                    _playerEquipDictionary.Add(userId, new int[] { -1, -1, -1, -1, -1, -1 });
                    return _playerEquipDictionary[userId];
                }
            }
        }

        //增加物品
        public List<InventoryInfo>? AddItem(int userId, int itemId, int count)
        {
            lock (_playerInventoryDictionary)
            {
                if(_playerInventoryDictionary.TryGetValue(userId, out var list))
                {
                    //增加数量
                    foreach(var info in list)
                    {
                        if(info.ItemId == itemId)
                        {
                            info.ItemCount += count;
                            return list;
                        }
                    }
                    //增加格子
                    list.Add(new InventoryInfo()
                    {
                        ItemId = itemId,
                        ItemCount = count
                    });
                    return list;
                }
                return null;
            }
        }

        //使用物品
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

        //增加装备
        public int AddEquip(int userId, ItemDefine itemDefine)
        {
            int[] list = null;
            if(_playerEquipDictionary.ContainsKey(userId))
            {
                list = _playerEquipDictionary[userId];
            }
            else
            {
                _playerEquipDictionary.Add(userId, new int[6] {-1, -1, -1, -1, -1, -1});
                list = _playerEquipDictionary[userId];
            }

            switch ((EquipType)itemDefine.EquipType)
            {
                case EquipType.Sword:
                    if (list[0] != -1 && list[1] != -1)
                    {
                        return -1;
                    }
                    else if (list[0] == -1)
                    {
                        list[0] = itemDefine.ID;
                        return 0;
                    }
                    else
                    {
                        list[1] = itemDefine.ID;
                        return 1;
                    }

                case EquipType.Armor:
                    if (list[2] != -1 && list[3] != -1)
                    {
                        return -1;
                    }
                    else if (list[2] == -1)
                    {
                        list[2] = itemDefine.ID;
                        return 2;
                    }
                    else
                    {
                        list[3] = itemDefine.ID;
                        return 3;
                    }
            }
            return -1;
        }

        //卸下装备
        public List<InventoryInfo>? RemoveEquip(int userId, RemoveEquipRequest removeEquipRequest)
        {
            if(_playerEquipDictionary.TryGetValue(userId, out var list))
            {
                var itemID = list[removeEquipRequest.EquipGridIndex];
                if(itemID == -1 || itemID != removeEquipRequest.ItemId)
                {
                    Log.Information("[InventoryManager] 卸下装备失败");
                    return null;
                }
                list[removeEquipRequest.EquipGridIndex] = -1;
                return AddItem(userId, removeEquipRequest.ItemId, 1);
            }
            return null;
        }
    }
}
