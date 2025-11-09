using Extension;
using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.GuildSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                GuildInfo guildInfo = createGuildRequest.GuildInfo;
                Log.Information($"[GuildService] 收到创建公会请求：{guildInfo.GuildName}");
                NetChannel? channel = sender as NetChannel;

                if(GuildManager.Instance.GetGuildByName(guildInfo.GuildName) != null)
                {
                    Log.Information($"[GuildService] 创建公会失败：重名");
                    channel?.SendAsync(new CreateGuildResponse() { IsSuccessfulCreateGuild = false });
                    return;
                }

                GuildManager.Instance.AddGuild(guildInfo.ToGuild());
                MysqlManager.Instance._freeSql.Insert<DbGuild>(guildInfo.ToDbGuild());
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
                string? sendName = channel?._user._dbUser.UserName;
                Log.Information($"[GuildService] 收到加入公会请求：{sendName}");

                Guild? guild = GuildManager.Instance.GetGuildByName(joinGuildRequest.GuildName);
                if (guild == null) return;

                //TODO：数据库操作
                if (!guild.needEnterCheck)
                {
                    //guild.memberList?.Add(sendName);
                    Log.Information($"[GuildService] 加入成员列表");
                }
                else
                {
                    //guild.applicationList.Add(sendName);
                    Log.Information($"[GuildService] 加入申请列表");
                }
            });
        }
    }
}
