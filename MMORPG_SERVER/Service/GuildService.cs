using Extension;
using MMORPG_SERVER.Common.Network;
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
                channel?.SendAsync(new CreateGuildResponse() { IsSuccessfulCreateGuild = true });
                Log.Information($"[GuildService] 创建公会成功：{guildInfo.GuildName}");
            });
        }
    }
}
