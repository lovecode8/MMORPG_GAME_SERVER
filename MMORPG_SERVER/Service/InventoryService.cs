using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.AttributeSystem;
using MMORPG_SERVER.System.InventorySystem;
using Serilog;

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
                var inventory = InventoryManager.Instance.GetPlayerInventory(userId);

                var response = new SearchInventoryResponse() { Inventory = new() };
                if(inventory != null)
                {
                    response.Inventory.InventoryList.AddRange(inventory);
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
                var inventory = InventoryManager.Instance.UseItem(userId, itemId);
                
                if(inventory != null)
                {
                    int changeValue = AttributeManager.Instance.GetConsumableValue(channel._user._player, itemId);
                    var response = new UseItemResponse()
                    {
                        SuccessfulUseItem = true,
                        ChangeValue = changeValue
                    };
                    response.InventoryList.AddRange(inventory);
                    channel.SendAsync(response);

                    //向其他玩家同步属性
                }
                else
                {
                    channel.SendAsync(new UseItemResponse() { SuccessfulUseItem = false });
                }
            });
        }
    }
}
