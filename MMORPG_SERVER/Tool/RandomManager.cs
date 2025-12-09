using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Tool
{
    public class RandomManager : Singleton<RandomManager>
    {
        private RandomManager() { }

        private Random _random = new();

        //在指定范围中获取随机数列表
        public List<int> GetRandomNumbersWithRange(int count, int min, int max)
        {
            HashSet<int> ans = new();

            while(ans.Count < count)
            {
                int num = _random.Next(min, max + 1);
                ans.Add(num);
            }
            return ans.ToList();
        }

        //获取随机Int值
        public int GetRandomInt()
        {
            return _random.Next();
        }
    }
}
