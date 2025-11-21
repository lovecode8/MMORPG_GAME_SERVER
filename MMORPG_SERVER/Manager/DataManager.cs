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

        private DataManager()
        {
            unitDictionary = new Dictionary<int, UnitDefine>();
        }

        public void Start()
        {
            unitDictionary = Load<Dictionary<int, UnitDefine>>("/UnitDefine.json");
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

        public UnitDefine GetUnitDefine(int unitId)
        {
            return unitDictionary[unitId];
        }
    }
}
