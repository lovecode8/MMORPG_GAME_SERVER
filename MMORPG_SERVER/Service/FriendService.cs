using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
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
                        IsOnline = FriendManager.Instance.IsTargetOnline(character.Name)
                    };
                    channel?.SendAsync(new SearchFriendResponse() { FriendInfo = friendInfo });
                }
            });
        }

        //处理添加好友请求
        public void OnHandle(object sender, AddFriendRequest addFriendRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                NetChannel? channel = sender as NetChannel;
                string _targetName = addFriendRequest.TargetName;
                string _senderName = channel._user._dbUser.UserName;
                Log.Information($"[FriendService] 收到好友添加请求：{_senderName}要加{_targetName}");

                if (!FriendManager.Instance.AddFriendApplication(_senderName, _targetName))
                {
                    //重复的好友申请不处理
                    return;
                }

                //存入数据库
                DbFriendApplication application = new() { senderName = _senderName, targetName = _targetName };
                MysqlManager.Instance._freeSql.Insert<DbFriendApplication>(application).ExecuteAffrows();
                User? user = UserManager.Instance.GetUserByName(_targetName);

                if(user != null)
                {
                    var character = FriendManager.Instance.GetCharacterByName(_senderName);
                    FriendInfo friendInfo = new FriendInfo()
                    {
                        CharacterId = character.UnitId,
                        UserName = _senderName,
                        IsOnline = true
                    };
                    user._netChannel.SendAsync(new AddFriendResponse() { FriendInfo = friendInfo });
                }
            });
        }

        //处理同意好友请求消息
        public void OnHandle(object sender, AgreeAddFriendRequest agreeAddFriendRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                NetChannel? channel = sender as NetChannel;
                string senderName = channel._user._dbUser.UserName;
                string targetName = agreeAddFriendRequest.TargetName;
                Log.Information($"[FriendService] 收到同意好友请求：{senderName}同意了{targetName}");

                FriendManager.Instance.RemoveApplication(senderName, targetName);
                FriendManager.Instance.AddFriend(senderName, targetName);
                FriendManager.Instance.AddFriend(targetName, senderName);

                //数据库操作--删除申请、增加好友
                MysqlManager.Instance._freeSql.Delete<DbFriendApplication>().
                                Where(f => f.targetName == senderName).
                                ExecuteAffrows();
                MysqlManager.Instance._freeSql.Insert<DbFriend>
                (new DbFriend() { name1 = targetName, name2 = senderName }).ExecuteAffrows();

                User user = UserManager.Instance.GetUserByName(targetName);
                if(user != null)
                {
                    var character = FriendManager.Instance.GetCharacterByName(senderName);
                    FriendInfo friendInfo = new FriendInfo()
                    {
                        CharacterId = character.UnitId,
                        UserName = senderName,
                        IsOnline = true
                    };
                    user._netChannel.SendAsync(new AgreeFriendResponse() { FriendInfo = friendInfo });
                }
            });
        }
    }
}
