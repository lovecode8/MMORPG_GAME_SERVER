using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.PlayerSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.UserSystem
{
    //用户类
    public class User
    {
        public int _userId => _dbUser.UserId;

        public DBUser _dbUser;

        public NetChannel _netChannel;

        public Player _player;

        public User(NetChannel channel, DBUser dbUser)
        {
            _netChannel = channel;
            _dbUser = dbUser;
        }

        public void SetPlayer(Player player)
        {
            _player = player;
        }
    }
}
