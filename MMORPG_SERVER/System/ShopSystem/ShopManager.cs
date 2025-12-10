using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.InventorySystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static FreeSql.Internal.GlobalFilter;

namespace MMORPG_SERVER.System.ShopSystem
{
    //商店管理器
    public class ShopManager : Singleton<ShopManager>
    {
        private ShopManager() { }

        //商品类型对应该类型商品列表
        private Dictionary<int, List<ShopItem>> _shopItemDict = new();

        //可供抽奖的物品列表
        private List<ShopItem> _drawItemList = new();

        //抽1次费用
        private int _drawOnePrice;

        //抽5次费用
        private int _drawFivePrice;

        public void Start()
        {
            LoadShopItems();
            LoadDrawItems();
        }

        //加载商店商品
        private void LoadShopItems()
        {
            var itemList = DataManager.Instance.GetAllItems();

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

        //加载可供抽奖的商品
        private void LoadDrawItems()
        {
            var allItems = DataManager.Instance.GetAllItems();
            allItems.Sort((i1, i2) => i1.ItemQuality.CompareTo(i2.ItemQuality));

            //普通取5个、稀有取3个、至尊取2个
            AddDrawItems(allItems, RandomManager.Instance.GetRandomNumbersWithRange(5, 0, 5));
            AddDrawItems(allItems, RandomManager.Instance.GetRandomNumbersWithRange(3, 6, 12));
            AddDrawItems(allItems, RandomManager.Instance.GetRandomNumbersWithRange(2, 13, 18));

            //获取抽奖花费
            int averagePrice = _drawItemList.Sum(i => i.Price) / 10;
            _drawOnePrice = averagePrice - averagePrice % 100;
            _drawFivePrice = (int)(_drawOnePrice * 4.5f);

            //打乱顺序
            _drawItemList = _drawItemList.OrderBy(i => RandomManager.Instance.GetRandomInt()).ToList();
        }

        private void AddDrawItems(List<ItemDefine> allItems, List<int> indexList)
        {
            foreach(var index in indexList)
            {
                var item = allItems[index];
                var shopItem = new ShopItem()
                {
                    ShopName = item.Name,
                    SpritePath = item.SpritePath,
                    Price = item.Price,
                    ItemId = item.ID,
                    ItemQuality = item.ItemQuality
                };
                _drawItemList.Add(shopItem);
            }
        }

        //根据商品类型获取商品
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

        //获取抽奖商品列表
        public List<ShopItem> GetDrawShopItemList()
        {
            return _drawItemList;
        }

        //获取抽奖花费
        public int GetDrawOnePrice()
        {
            return _drawOnePrice;
        }

        public int GetDrawFivePrice()
        {
            return _drawFivePrice;
        }

        //抽奖算法--返回抽中的商品列表
        public List<DrawedItem> GetDrawItem(int count)
        {
            List<DrawedItem> ans = new();

            for(int i = 0; i < count; i++)
            {
                int index = RandomManager.Instance.GetRandomInt(0, 9);
                var item = _drawItemList[index];
                var drawedItem = ans.Find(i => i.ItemName == item.ShopName);
                if(drawedItem != null)
                {
                    drawedItem.Count++;
                }
                else
                {
                    ans.Add(new DrawedItem()
                    {
                        ItemName = item.ShopName,
                        SpritePath = item.SpritePath,
                        ItemQuality = item.ItemQuality,
                        TargetIndex = index,
                        Count = 1,
                        ItemId = item.ItemId
                    });
                }
                Log.Information(item.ShopName);
            }
            return ans;
        }
    }
}
