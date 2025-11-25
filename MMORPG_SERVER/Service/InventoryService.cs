using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.AttributeSystem;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.InventorySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.PlayerSystem;
using Serilog;
using System.Numerics;
using static FreeSql.Internal.GlobalFilter;

namespace MMORPG_SERVER.Service
{
    public class InventoryService : ServiceBase<InventoryService>
    {
        //处理加载用户库存信息请求
        public void OnHandle(object sender, SearchInventoryRequest searchInventoryRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                int userId = channel._user._userId;
                Log.Information($"[InventoryService] 收到加载库存请求：{userId}");

                var response = new SearchInventoryResponse(){ Inventory = new() };
                var inventory = InventoryManager.Instance.GetPlayerInventory(userId);
                var equipList = InventoryManager.Instance.GetPlayerEquip(userId);

                if (inventory != null)
                {
                    response.Inventory.InventoryList.AddRange(inventory);
                }

                if(equipList != null)
                {
                    response.EquipList.AddRange(equipList);
                }
                channel.SendAsync(response);
            });
        }

        //处理玩家使用物品请求
        public void OnHandle(object sender, UseItemRequest useItemRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                //Hp、Mp、MaxHp、MaxMp才需要同步至客户端, Atk等由服务端保存
                var channel = sender as NetChannel;
                int userId = channel._user._userId;
                var itemId = useItemRequest.ItemId;
                var itemDefine = DataManager.Instance.GetItemDefine(itemId);

                var inventory = InventoryManager.Instance.UseItem(userId, itemId);
                
                if(inventory != null)
                {
                    var response = new UseItemResponse()
                    {
                        SuccessfulUseItem = true
                    };
                    response.InventoryList.AddRange(inventory);

                    //使用消耗品
                    if ((ItemType)itemDefine.ItemType == ItemType.Consunable)
                    {
                        int changeValue = AttributeManager.Instance.GetConsumableValue(channel._user._player, itemDefine);
                        response.ChangeValue = changeValue;

                        //向其他玩家同步属性（仅同步Hp和maxHp）
                        if((ConsumableType)itemDefine.ConsumableType == ConsumableType.Hp || 
                        (ConsumableType)itemDefine.ConsumableType == ConsumableType.MaxMp)
                        {
                            PlayerManager.Instance.Broadcast(new SyncAttributeResponse()
                            {
                                PlayerId = channel._user._player._playerId,
                                ConsumableType = itemDefine.ConsumableType,
                                ChangeValue = changeValue
                            }, channel._user._player);
                        }
                        
                    }
                    //使用装备
                    else
                    {
                        int gridIndex = InventoryManager.Instance.AddEquip(userId, itemDefine);
                        if (gridIndex == -1)
                        {
                            //使用装备失败--满了
                            channel.SendAsync(new UseItemResponse() { SuccessfulUseItem = false });
                            return;
                        }
                        MysqlManager.Instance._freeSql.Insert<DbEquip>(new DbEquip()
                        {
                            ownerId = userId,
                            gridIndex = gridIndex,
                            itemId = itemId
                        }).ExecuteAffrows();
                        AttributeManager.Instance.OnUseEquip(userId, itemDefine);
                    }
                    channel.SendAsync(response);
                }
                else
                {
                    channel.SendAsync(new UseItemResponse() { SuccessfulUseItem = false });
                }
            });
        }

        //处理玩家卸下装备请求
        public void OnHandle(object sender, RemoveEquipRequest removeEquipRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                int userId = channel._user._userId;
                var inventoryInfoList =
                    InventoryManager.Instance.RemoveEquip(userId, removeEquipRequest);
                var response = new RemoveEquipResponse() { Inventory = new() };
                if(inventoryInfoList != null)
                {
                    response.SuccessfulRemove = true;
                    response.Inventory.InventoryList.AddRange(inventoryInfoList);
                    AttributeManager.Instance.OnRemoveEquip(userId, removeEquipRequest.ItemId);
                    MysqlManager.Instance._freeSql.Delete<DbEquip>().
                        Where(e => e.ownerId == userId).
                        Where(e => e.gridIndex == removeEquipRequest.EquipGridIndex).
                        Where(e => e.itemId == removeEquipRequest.ItemId).
                        ExecuteAffrows();
                }
                else
                {
                    response.SuccessfulRemove = false;
                }
                channel.SendAsync(response);
            });
        }

        //处理玩家丢弃物品请求
        public void OnHandle(object sender, DropItemRequest dropItemRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                int userId = channel._user._userId;
                int itemId = dropItemRequest.ItemId;
                var itemDefine = DataManager.Instance.GetItemDefine(itemId);
                var player = channel._user._player;

                Log.Information($"[InventoryService] 收到玩家丢弃物品请求：{userId} 丢弃 {itemId}");

                var inventory = InventoryManager.Instance.UseItem(userId, itemId);
                var response = new DropItemResponse() { Inventory = new() };
                if (inventory == null)
                {
                    response.IsSuccessfulDrop = false;
                    channel.SendAsync(response);
                }
                else
                {
                    response.IsSuccessfulDrop = true;
                    response.Inventory.InventoryList.AddRange(inventory);
                    channel.SendAsync(response);

                    //在场景中创建物品
                    var entity = new Entity(EntityManager.Instance.NewEntityId(),
                        EntityType.Item,
                        DataManager.Instance.GetUnitDefine(itemDefine.UnitId),
                        player._position + new Vector3(0, 1, 0),
                        0);
                    EntityManager.Instance.AddEntity(entity);
                    MapManager.Instance.EntityEnter(entity);
                }
            });
        }
    }
}
