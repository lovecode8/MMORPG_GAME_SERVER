using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Tool;
using Serilog;
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

        public void Start()
        {
            LoadGuildFromDatabase();
        }

        //从数据库加载公会信息
        private void LoadGuildFromDatabase()
        {
            List<DbGuild> dbGuildList = MysqlManager.Instance._freeSql.Select<DbGuild>().ToList();
            foreach (DbGuild dbGuild in dbGuildList)
            {
                _guildDictionary.Add(dbGuild.guildName, dbGuild.ToGuild());
            }
        }

        //获取公会
        public Guild? GetGuildByName(string name)
        {
            if (_guildDictionary.TryGetValue(name, out var guild))
            {
                return guild;
            }
            return null;
        }

        //增加公会
        public void AddGuild(string senderName, Guild guild)
        {
            guild.memberList?.Add(senderName);
            lock (_guildDictionary)
            {
                _guildDictionary.Add(guild.guildName, guild);
            }
        }

        //会员退出公会
        public Guild? ExitGuild(string senderName, string guildName)
        {
            lock (_guildDictionary)
            {
                var guild = GetGuildByName(guildName);
                if (guild == null || !guild.memberList.Contains(senderName)) return null;

                guild.memberList.Remove(guildName);
                return guild;
            }
        }
    }
}
