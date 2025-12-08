using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
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
            });
        }
    }
}
