using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.InventorySystem;
using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.ShopSystem
{
    //商店管理器
    public class ShopManager : Singleton<ShopManager>
    {
        private ShopManager() { }

        //商品类型对应该类型商品列表
        private Dictionary<int, List<ShopItem>> _shopItemDict = new();

        public void Start()
        {
            LoadShopItem();
        }

        private void LoadShopItem()
        {
            var itemList = DataManager.Instance.itemDefineDictionary.Values;

            foreach(var item in itemList)
            {
                var shopItem = new ShopItem()
                {
                    ShopName = item.Name,
                    SpritePath = item.SpritePath,
                    Price = item.Price,
                    ItemId = item.ID
                };
                if (!_shopItemDict.ContainsKey(item.ItemType))
                {
                    _shopItemDict[item.ItemType] = new();
                }

                _shopItemDict[item.ItemType].Add(shopItem);
            }
        }

        public List<ShopItem> GetShopItemByShopType(int shopType)
        {
            var ans = new List<ShopItem>();
            if(shopType == 0)
            {
                foreach(var list in _shopItemDict.Values)
                {
                    ans.AddRange(list);
                }
            }
            else
            {
                ans.AddRange(_shopItemDict[shopType - 1]);
            }
            return ans;
        }
    }
}
