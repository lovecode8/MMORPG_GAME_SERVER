using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.FriendSystem;
using MMORPG_SERVER.System.UserSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Service
{
    //处理好友相关服务
    public class FriendService : ServiceBase<FriendService>
    {
        //处理搜索好友请求
        public void OnHandle(object sender, SearchFriendRequest searchFriendRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                NetChannel? channel = sender as NetChannel;
                string userName = searchFriendRequest.UserName;
                Log.Information($"[FriendService] 收到搜索好友请求: {userName}");

                var character = FriendManager.Instance.GetCharacterByName(userName);
                if(character == null)
                {
                    channel?.SendAsync(new SearchFriendResponse());
                }
                else
                {
                    FriendInfo friendInfo = new()
                    {
                        CharacterId = character.UnitId,
                        UserName = character.Name,
                        IsOnline =
                        UserManager.Instance.GetUserByName(character.Name) == null ? false : true
                    };
                    channel?.SendAsync(new SearchFriendResponse() { FriendInfo = friendInfo });
                }
            });
        }

        public void OnHandle(object sender, AddFriendRequest addFriendRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                NetChannel? channel = sender as NetChannel;
            });
        }
    }
}
