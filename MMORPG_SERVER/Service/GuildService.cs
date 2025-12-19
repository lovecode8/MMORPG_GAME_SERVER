using Extension;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.ChatSystem;
using MMORPG_SERVER.System.FriendSystem;
using MMORPG_SERVER.System.GuildSystem;
using MMORPG_SERVER.System.TaskSystem;
using MMORPG_SERVER.System.UserSystem;
using Org.BouncyCastle.Tls;
using Serilog;
using System;

namespace MMORPG_SERVER.Service
{
    //处理公会相关服务
    public class GuildService : ServiceBase<GuildService>
    {
        //处理加载我的公会请求
        public void OnHandle(object sender, SearchMyGuildRequest searchMyGuildRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                string senderName = channel._user._dbUser.UserName;
                Log.Information($"[GuildService] 收到加载我的公会请求：{senderName}");

                var myGuildInfo = GuildManager.Instance.GetGuildByUserName(senderName);

                if(myGuildInfo != null)
                {
                    var chatMessageList = ChatManager.Instance.GetGuildChatMessage(myGuildInfo.GuildName);
                    //导入聊天消息
                    if(chatMessageList != null)
                    {
                        myGuildInfo?.MessageList.AddRange(chatMessageList);
                    }
                }
                    
                channel.SendAsync(new SearchMyGuildResponse() { GuildInfo = myGuildInfo ?? null });
                Log.Information($"[GuildService] 查询结果：{myGuildInfo?.GuildName ?? "没有公会"}");
            });
        }

        //处理创建公会请求
        public void OnHandle(object sender, CreateGuildRequest createGuildRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                NetChannel? channel = sender as NetChannel;
                var guildInfo = createGuildRequest.GuildInfo;
                string senderName = channel._user._dbUser.UserName;
                Log.Information($"[GuildService] 收到创建公会请求：{guildInfo.GuildName}");

                if (GuildManager.Instance.GetGuildByName(guildInfo.GuildName) != null)
                {
                    Log.Information($"[GuildService] 创建公会失败：重名");
                    channel?.SendAsync(new CreateGuildResponse() { IsSuccessfulCreateGuild = false });
                    return;
                }

                GuildManager.Instance.AddGuild(senderName, guildInfo.ToGuild());
                MysqlManager.Instance._freeSql.Insert<DbGuild>(guildInfo.ToDbGuild()).ExecuteAffrows();
                MysqlManager.Instance._freeSql.Insert<DbGuildMember>(new DbGuildMember()
                {
                    userName = senderName,
                    guildName = guildInfo.GuildName
                }).ExecuteAffrows();

                //更新任务
                TaskManager.Instance.UpdateTask(channel._user._userId, 4, 1);

                channel?.SendAsync(new CreateGuildResponse() { IsSuccessfulCreateGuild = true });
                Log.Information($"[GuildService] 创建公会成功：{guildInfo.GuildName}");
            });
        }

        //处理搜索公会请求
        public void OnHandle(Object sender, SearchGuildRequest searchGuildRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                NetChannel? channel = sender as NetChannel;
                Log.Information($"[GuildService] 收到搜索公会请求：{searchGuildRequest.GuildName}");

                Guild? guild = GuildManager.Instance.GetGuildByName(searchGuildRequest.GuildName);
                if(guild == null)
                {
                    channel?.SendAsync(new SearchGuildResponse());
                }
                else
                {
                    channel?.SendAsync(new SearchGuildResponse() { GuildInfo = guild.ToGuildInfo()});
                }
            });
        }

        //处理加入公会请求
        public void OnHandle(Object sender, JoinGuildRequest joinGuildRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                var senderName = channel?._user._dbUser.UserName;
                Log.Information($"[GuildService] 收到加入公会请求：{senderName}");

                var guild = GuildManager.Instance.UserEnterGuild(joinGuildRequest.GuildName, senderName);
                if(guild != null)
                {
                    if (!guild.needEnterCheck)
                    {
                        //更新任务进度
                        TaskManager.Instance.UpdateTask(channel._user._userId, 4, 1);

                        //无需审核：发给所有在线同会玩家客户端
                        SendMessageToGuildMember(guild.memberList, new JoinGuildResponse()
                        {
                            IsEnter = true,
                            FriendInfo = FriendManager.Instance.GetFriendInfoByName(senderName)
                        });
                    }
                    else
                    {
                        //需要审核：发给会长客户端（若在线）
                        var user = UserManager.Instance.GetUserByName(guild.ownerName);
                        if(user != null)
                        {
                            user._netChannel.SendAsync(new JoinGuildResponse()
                            {
                                IsEnter = false,
                                FriendInfo = FriendManager.Instance.GetFriendInfoByName(senderName)
                            });
                        }
                    }
                }
                
            });
        }

        //处理会长同意玩家加入公会请求
        public void OnHandle(object sender, AgreeEnterGuildRequest agreeEnterGuildRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                NetChannel channel = sender as NetChannel;
                string targetName = agreeEnterGuildRequest.TargetName;
                string guildName = agreeEnterGuildRequest.GuildName;
                var guild = GuildManager.Instance.AgreeEnterGuild(targetName, guildName);
                if (guild == null) return;

                MysqlManager.Instance._freeSql.Insert<DbGuildMember>(new DbGuildMember()
                {
                    userName = targetName,
                    guildName = guildName
                }).ExecuteAffrows();
                MysqlManager.Instance._freeSql.Delete<DbGuildApplication>()
                    .Where(a => a.senderName == targetName).ExecuteAffrows();
                MysqlManager.Instance._freeSql.Update<DbGuild>()
                    .Where(g => g.guildName == guildName)
                    .Set(g => g.count, guild.count).ExecuteAffrows();

                SendMessageToGuildMember(guild.memberList, new JoinGuildResponse()
                {
                    IsEnter = true,
                    FriendInfo = FriendManager.Instance.GetFriendInfoByName(targetName)
                });

                //更新任务进度
                var targetUserId = MysqlManager.Instance._freeSql.Select<DBUser>().
                    Where(u => u.UserName == targetName).First().UserId;
                TaskManager.Instance.UpdateTask(targetUserId, 4, 1);

                //通知申请人（若在线）
                var targetUser = UserManager.Instance.GetUserByName(targetName);
                targetUser?._netChannel.SendAsync(new AgreeEnterGuildResponse()
                {
                    GuildInfo = guild.ToGuildInfo()
                });
            });
        }

        //处理退出公会请求
        public void OnHandle(object sender, ExitGuildRequest exitGuildRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                NetChannel? channel = sender as NetChannel;
                var senderName = channel?._user._dbUser.UserName;
                var guildName = exitGuildRequest.GuildName;
                Log.Information($"[GuildService] 收到退出公会请求：{senderName}退出{guildName}");

                var guild = GuildManager.Instance.ExitGuild(senderName, guildName);
                if(guild != null)
                {
                    MysqlManager.Instance._freeSql.Update<DbGuild>()
                        .Where(g => g.guildName == guildName)
                        .Set(g => g.count, guild.count).ExecuteAffrows();
                    MysqlManager.Instance._freeSql.Delete<DbGuildMember>()
                        .Where(m => m.userName == senderName).ExecuteAffrows();

                    SendMessageToGuildMember(guild.memberList, new ExitGuildResponse()
                    {
                        SenderName = senderName
                    });
                }
            });
        }

        //处理会长移除会员请求
        public void OnHandle(object sender, RemoveGuildMemberRequest removeGuildMemberRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                NetChannel channel = sender as NetChannel;
                string senderName = channel._user._dbUser.UserName;
                string guildName = removeGuildMemberRequest.GuildName;
                string targetName = removeGuildMemberRequest.TargetName;
                if (guildName == null || targetName == null) return;

                var guild = GuildManager.Instance.ExitGuild(targetName, guildName);
                if(guild != null)
                {
                    MysqlManager.Instance._freeSql.Update<DbGuild>()
                        .Where(g => g.guildName == guildName)
                        .Set(g => g.count, guild.count)
                        .ExecuteAffrows();
                    MysqlManager.Instance._freeSql.Delete<DbGuildMember>()
                        .Where(m => m.userName == targetName)
                        .ExecuteAffrows();

                    SendMessageToGuildMember(guild.memberList, new RemoveGuildMemberResponse()
                    {
                        TargetName = targetName
                    });
                }
            });
        }

        private void SendMessageToGuildMember(List<string> memberList, IMessage message)
        {
            //向在线的公会成员发送退出信息
            foreach (var userName in memberList)
            {
                var user = UserManager.Instance.GetUserByName(userName);
                user?._netChannel.SendAsync(message);
            }
        }
    }
}
