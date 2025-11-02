using MMORPG_SERVER.Tool;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MMORPG_SERVER.Manager
{
    //服务器固定数据管理器
    public class DataManager : Singleton<DataManager>
    {
        public Dictionary<int, CharacterDefine> characterDictionary;

        private DataManager()
        {
            characterDictionary = new Dictionary<int, CharacterDefine>();
        }

        public void Start()
        {
            characterDictionary = Load<Dictionary<int, CharacterDefine>>("/CharacterDefine.json");
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

        public CharacterDefine GetCharacterDefine(int characterId)
        {
            return characterDictionary[characterId];
        }
    }
}
