using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.Tool;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;

namespace MMORPG_SERVER.Manager
{
    //服务器固定数据管理器
    public class DataManager : Singleton<DataManager>
    {
        public Dictionary<int, UnitDefine> unitDictionary;

        public Dictionary<int, ItemDefine> itemDefineDictionary;

        public Dictionary<int, TaskDefine> taskDefineDictionary;

        public Dictionary<int, TalkDefine> talkDefineDictionary;

        private DataManager()
        {
            unitDictionary = new Dictionary<int, UnitDefine>();
            itemDefineDictionary = new Dictionary<int, ItemDefine>();
            taskDefineDictionary = new Dictionary<int, TaskDefine>();
            talkDefineDictionary = new Dictionary<int, TalkDefine>();
        }

        public void Start()
        {
            unitDictionary = Load<Dictionary<int, UnitDefine>>("/UnitDefine.json");
            itemDefineDictionary = Load<Dictionary<int, ItemDefine>>("ItemDefine.json");
            taskDefineDictionary = Load<Dictionary<int, TaskDefine>>("TaskDefine.json");
            talkDefineDictionary = Load<Dictionary<int, TalkDefine>>("TalkDefine.json");
        }

        public void Update()
        {

        }

        private T Load<T>(string jsonPath)
        {
            string content = LoadFromFile(jsonPath);
            Debug.Assert(content != null);
            var res = JsonConvert.DeserializeObject<T>(content);
            return res;
        }

        private string LoadFromFile(string path)
        {
            string jsonPath = "D:\\game\\MMORPG_GAME_SERVER\\MMORPG_SERVER\\Data\\Json";
            string content = File.ReadAllText(Path.Join(jsonPath, path));
            return content;
        }

        public List<ItemDefine> GetAllItems()
        {
            lock(itemDefineDictionary)
            {
                return itemDefineDictionary.Values.ToList();
            }
        }

        public UnitDefine GetUnitDefine(int unitId)
        {
            lock (unitDictionary)
            {
                return unitDictionary[unitId];
            }
        }

        public ItemDefine GetItemDefine(int itemId)
        {
            lock(itemDefineDictionary)
            {
                return itemDefineDictionary[itemId];
            }
        }

        public TaskDefine GetTaskDefine(int taskId)
        {
            lock (taskDefineDictionary)
            {
               return taskDefineDictionary[taskId];
            }
        }

        public TalkDefine GetTalkDefine(int talkId)
        {
            lock(talkDefineDictionary)
            {
                return talkDefineDictionary[talkId];
            }
        }
    }
}
