using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.System.UserSystem;
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
        private Dictionary<string, List<string>> _userFriendApplicationDict = new();
        
        //玩家的好友列表
        private Dictionary<string, List<string>> _userFriendList = new();

        private FriendManager() { }

        public void Start()
        {
            LoadDataFromDatabase();
        }

        //导入数据
        private void LoadDataFromDatabase()
        {
            _dbCharacterDictionary =
                MysqlManager.Instance._freeSql.Select<DbCharacter>()
                .ToDictionary(c => c.Name, c => c);

            List<DbFriendApplication> applicationList =
                MysqlManager.Instance._freeSql.Select<DbFriendApplication>().ToList();
            foreach(DbFriendApplication application in applicationList)
            {
                AddFriendApplication(application.senderName, application.targetName);
            }

            List<DbFriend> friendList = MysqlManager.Instance._freeSql.Select<DbFriend>().ToList();
            foreach(DbFriend dbFriend in friendList)
            {
                AddFriend(dbFriend.name1, dbFriend.name2);
                AddFriend(dbFriend.name2, dbFriend.name1);
            }
        }

        public DbCharacter GetCharacterByName(string userName)
        {
            if (_dbCharacterDictionary.TryGetValue(userName, out var character))
            {
                return character;
            }
            return null;
        }

        //判断好友是否在线
        public bool IsTargetOnline(string userName)
        {
            return UserManager.Instance.GetUserByName(userName) != null;
        }

        //添加好友请求
        public bool AddFriendApplication(string senderName, string targetName)
        {
            //已经是好友则不转发好友请求
            if(_userFriendList.ContainsKey(senderName) && _userFriendList[senderName].Contains(targetName))
            {
                return false;
            }

            if (_userFriendApplicationDict.TryGetValue(targetName, out var list))
            {
                if (list.Contains(senderName))
                {
                    return false;
                }
                list.Add(senderName);
            }
            else
            {
                _userFriendApplicationDict.Add(targetName, new());
                _userFriendApplicationDict[targetName].Add(senderName);
            }
            return true;
        }

        //移除好友请求
        public void RemoveApplication(string senderName, string targetName)
        {
            _userFriendApplicationDict[senderName].Remove(targetName);
        }

        //添加好友
        public void AddFriend(string senderName, string targetName)
        {
            if(_userFriendList.TryGetValue(senderName, out var list) && !list.Contains(targetName))
            {
                list.Add(targetName);
            }
            else
            {
                _userFriendList.Add(senderName, new());
                _userFriendList[senderName].Add(targetName);
            }
        }
    }
}
