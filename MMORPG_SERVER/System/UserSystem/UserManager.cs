using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.UserSystem
{
    //用户管理器
    public class UserManager : Singleton<UserManager>
    {
        private Dictionary<string, User> _userDictionary = new();

        private UserManager() { }

        public void Start()
        {
            
        }

        public User NewUser(NetChannel netChannel, DBUser dbUser)
        {
            User user = new User(netChannel, dbUser);
            _userDictionary.Add(dbUser.UserName, user);
            return user;
        }

        public User? GetUserByName(string userName)
        {
            if(_userDictionary.TryGetValue(userName, out User? user))
            {
                return user;
            }
            return null;
        }

        public User? GetUserById(int id)
        {
            foreach(var user in _userDictionary.Values)
            {
                if (user._userId == id) return user;
            }
            return null;
        }

        public void RemoveUser(string userName)
        {
            if(_userDictionary.ContainsKey(userName))
            {
                _userDictionary.Remove(userName);
            }
        }
    }
}
