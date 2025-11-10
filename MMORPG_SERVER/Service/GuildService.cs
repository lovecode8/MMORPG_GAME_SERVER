using Extension;
using Google.Protobuf.Reflection;
using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.GuildSystem;
using MMORPG_SERVER.System.UserSystem;
using Org.BouncyCastle.Tls;
using Serilog;

namespace MMORPG_SERVER.Service
{
    //处理公会相关服务
    public class GuildService : ServiceBase<GuildService>
    {
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
                NetChannel? channel = sender as NetChannel;
                var senderName = channel?._user._dbUser.UserName;
                Log.Information($"[GuildService] 收到加入公会请求：{senderName}");

                var guild = GuildManager.Instance.GetGuildByName(joinGuildRequest.GuildName);
                if (guild == null) return;

                //TODO：数据库操作
                if (!guild.needEnterCheck)
                {
                    guild.memberList?.Add(senderName);
                    MysqlManager.Instance._freeSql.Insert<DbGuildMember>(new DbGuildMember()
                    {
                        userName = senderName,
                        guildName = guild.guildName
                    }).ExecuteAffrows();
                    Log.Information($"[GuildService] 加入成员列表");
                }
                else
                {
                    guild.applicationList.Add(senderName);
                    MysqlManager.Instance._freeSql.Insert<DbGuildApplication>(new DbGuildApplication
                    {
                        senderName = senderName,
                        guildName = guild.guildName
                    }).ExecuteAffrows();
                    Log.Information($"[GuildService] 加入申请列表");
                }
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
                MysqlManager.Instance._freeSql.Update<DbGuild>(guild.ToDbGuild()).ExecuteAffrows();
                MysqlManager.Instance._freeSql.Delete<DbGuildMember>().Where(m => m.userName == senderName).ExecuteAffrows();
                if(guild != null)
                {
                    //向在线的公会成员发送退出信息
                    foreach(var userName in guild.memberList)
                    {
                        var user = UserManager.Instance.GetUserByName(userName);
                        if(user != null)
                        {
                            user._netChannel.SendAsync(new ExitGuildResponse() { SenderName = senderName });
                        }
                    }
                }
            });
        }
    }
}
