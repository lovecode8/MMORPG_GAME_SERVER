using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.InventorySystem;
using MMORPG_SERVER.System.ShopSystem;
using Serilog;

namespace MMORPG_SERVER.Service
{
    //处理商店有关消息
    public class ShopService : ServiceBase<ShopService>
    {
        //处理获取商品列表请求
        public void OnHandle(object sender, SelectShopItemRequest selectShopItemRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                var shopType = selectShopItemRequest.ShopType;
                Log.Information($"收到获取商品请求：{channel._user._userId}");

                var response = new SelectShopResponse();
                response.ShopItemList.AddRange(ShopManager.Instance.GetShopItemByShopType(shopType));
                channel.SendAsync(response);
                Log.Information(response.ShopItemList.Count.ToString());
            });
        }

        //处理购买商品请求
        public void OnHandle(object sender, BuyShopItemRequest buyShopItemRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                int itemId = buyShopItemRequest.ItemId;

                Log.Information($"收到购买商品请求：{channel._user._userId}买{itemId}");
                var playerGold = channel._user._player._dbCharacter.Gold;
                var itemPrice = DataManager.Instance.GetItemDefine(itemId).Price;
                var response = new BuyShopItemResponse() { Inventory = new() };

                //金币不足
                if(itemPrice > playerGold)
                {
                    response.IsSuccessfulBuy = false;
                    channel.SendAsync(response);
                }
                else
                {
                    var itemList = InventoryManager.Instance.AddItem(channel._user._userId, itemId, 1);
                    var gold = playerGold - itemPrice;
                    channel._user._player._dbCharacter.Gold = gold;
                    response.IsSuccessfulBuy = true;
                    response.Inventory.InventoryList.AddRange(itemList);
                    response.Gold = gold;
                    channel.SendAsync(response);
                }
            });
        }

        //处理获取抽奖物品列表请求
        public void OnHandle(object sender, SelectDrawItemRequest selectDrawItemRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                var response = new SelectDrawItemResponse()
                {
                    DrawOneGold = ShopManager.Instance.GetDrawOnePrice(),
                    DrawFiveGold = ShopManager.Instance.GetDrawFivePrice()
                };
                response.ItemList.AddRange(ShopManager.Instance.GetDrawShopItemList());
                channel?.SendAsync(response);
            });
        }
    }
}
