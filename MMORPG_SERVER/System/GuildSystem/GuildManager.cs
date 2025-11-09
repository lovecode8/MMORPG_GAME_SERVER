using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.GuildSystem
{
    //公会管理器 
    public class GuildManager : Singleton<GuildManager>
    {
        private GuildManager() { }

        //公会字典--名字对应公会
        private Dictionary<string, Guild> _guildDictionary = new();

        //获取公会
        public Guild? GetGuildByName(string name)
        {
            if(_guildDictionary.TryGetValue(name, out var guild))
            {
                return guild;
            }
            return null;
        }

        //增加公会
        public void AddGuild(Guild guild)
        {
            _guildDictionary.Add(guild.guildName, guild);
        }
    }
}
