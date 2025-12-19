using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.ChatSystem;
using MMORPG_SERVER.System.FriendSystem;
using MMORPG_SERVER.System.TaskSystem;
using MMORPG_SERVER.System.UserSystem;
using Serilog;


namespace MMORPG_SERVER.Service
{
    //处理好友相关服务
    public class FriendService : ServiceBase<FriendService>
    {
        //处理加载好友相关信息的请求
        public void OnHandle(object sender, LoadFriendInfoRequest loadFriendInfoRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                NetChannel channel = sender as NetChannel;
                string senderName = loadFriendInfoRequest.UserName;
                Log.Information($"[FriendService] 收到加载好友信息请求：{senderName}");

                LoadFriendInfoResponse response = new();

                List<FriendInfo> applicationList = FriendManager.Instance.GetFriendApplication(senderName);
                if(applicationList != null)
                {
                    response.FriendApplicationList.AddRange(applicationList);
                }

                List<FriendInfo> friendList = FriendManager.Instance.GetFriendList(senderName);
                if(friendList != null)
                {
                    response.FriendList.AddRange(friendList);
                }

                Dictionary<string, ChatMessageList> messageDict = 
                    ChatManager.Instance.GetFriendChatMessageDict(senderName);
                if(messageDict != null)
                {
                    foreach (var kv in messageDict)
                    {
                        response.FriendMessageDict.Add(kv.Key, kv.Value);
                    }
                }
                
                channel.SendAsync(response);
            });
        }
        //处理搜索好友请求
        public void OnHandle(object sender, SearchFriendRequest searchFriendRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                string userName = searchFriendRequest.UserName;
                Log.Information($"[FriendService] 收到搜索好友请求: {userName}");

                var friendInfo = FriendManager.Instance.GetFriendInfoByName(userName);

                //不存在该玩家或是自己
                if(friendInfo == null || channel?._user._dbUser.UserName == userName)
                {
                    channel?.SendAsync(new SearchFriendResponse());
                }
                else
                {
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
                string targetName = addFriendRequest.TargetName;
                string senderName = channel._user._dbUser.UserName;
                Log.Information($"[FriendService] 收到好友添加请求：{senderName}要加{targetName}");

                //重复的好友申请不处理或自己申请自己
                if (!FriendManager.Instance.AddFriendApplication(senderName, targetName) || 
                    senderName == targetName)
                {
                    return;
                }

                //存入数据库
                DbFriendApplication application = new() { senderName = senderName, targetName = targetName };
                MysqlManager.Instance._freeSql.Insert<DbFriendApplication>(application).ExecuteAffrows();
                User? user = UserManager.Instance.GetUserByName(targetName);

                if(user != null)
                {
                    var friendInfo = FriendManager.Instance.GetFriendInfoByName(senderName);
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

                if (!FriendManager.Instance.RemoveApplication(senderName, targetName) ||
                !FriendManager.Instance.AddFriend(senderName, targetName) ||
                !FriendManager.Instance.AddFriend(targetName, senderName)) return;

                //数据库操作--删除申请、增加好友
                MysqlManager.Instance._freeSql.Delete<DbFriendApplication>().
                                Where(f => f.targetName == senderName).
                                ExecuteAffrows();

                MysqlManager.Instance._freeSql.Insert<DbFriend>
                (new DbFriend() { name1 = targetName, name2 = senderName }).ExecuteAffrows();

                User user = UserManager.Instance.GetUserByName(targetName);

                //更新任务进度
                //发送者
                TaskManager.Instance.UpdateTask(channel._user._userId, 3, 1);
                //目标用户
                var targetUserId = MysqlManager.Instance._freeSql.Select<DBUser>().
                    Where(u => u.UserName == targetName).First().UserId;
                TaskManager.Instance.UpdateTask(targetUserId, 3, 1);

                if(user != null)
                {
                    var friendInfo = FriendManager.Instance.GetFriendInfoByName(senderName);
                    user._netChannel.SendAsync(new AgreeFriendResponse() { FriendInfo = friendInfo });
                }
            });
        }
    }
}
