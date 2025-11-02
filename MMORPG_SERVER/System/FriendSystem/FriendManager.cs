using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.FriendSystem
{
    public class FriendManager : Singleton<FriendManager>
    {
        //所有用户角色字典--用于查找好友
        private Dictionary<string, DbCharacter> _dbCharacterDictionary = new();

        //玩家好友申请列表--记录一个玩家的所有好友申请
        private Dictionary<string, List<string>> _userFriendApplicationDictionary = new();

        private FriendManager() { }

        public void Start()
        {
            _dbCharacterDictionary = 
                MysqlManager.Instance._freeSql.Select<DbCharacter>()
                .ToDictionary(c => c.Name, c => c);
        }

        public DbCharacter GetCharacterByName(string userName)
        {
            if (_dbCharacterDictionary.TryGetValue(userName, out var character))
            {
                return character;
            }
            return null;
        }
    }
}
