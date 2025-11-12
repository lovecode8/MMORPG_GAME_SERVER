using Extension;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Tool;
using Org.BouncyCastle.Tls;
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
                string guildName = dbGuild.guildName;
                _guildDictionary.Add(guildName, dbGuild.ToGuild());

                var applicationList = MysqlManager.Instance._freeSql.Select<DbGuildApplication>()
                    .Where(a => a.guildName == guildName).ToList(a => a.senderName);
                _guildDictionary[guildName].applicationList.AddRange(applicationList);

                var memberList = MysqlManager.Instance._freeSql.Select<DbGuildMember>()
                    .Where(m => m.guildName == guildName).ToList(m => m.userName);
                _guildDictionary[guildName].memberList.AddRange(memberList);
            }
        }

        //根据用户名字查找他的公会信息
        public GuildInfo? GetGuildByUserName(string senderName)
        {
            var dbGuildMember = MysqlManager.Instance._freeSql.Select<DbGuildMember>()
                .Where(m => m.userName == senderName).First();
            if (dbGuildMember == null) return null;
            var guild = GetGuildByName(dbGuildMember.guildName);
            return guild.ToGuildInfo();
        }

        //获取公会
        public Guild? GetGuildByName(string name)
        {
            lock (_guildDictionary)
            {
                if (_guildDictionary.TryGetValue(name, out var guild))
                {
                    return guild;
                }
                return null;
            }
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

        //玩家加入公会
        public Guild UserEnterGuild(string guildName, string senderName)
        {
            var guild = GuildManager.Instance.GetGuildByName(guildName);
            if (guild == null) return null;

            if (!guild.needEnterCheck)
            {
                guild.memberList?.Add(senderName);
                guild.count++;
                MysqlManager.Instance._freeSql.Update<DbGuild>()
                    .Where(g => g.guildName == guildName)
                    .Set(g => g.count, guild.count)
                    .ExecuteAffrows();
                MysqlManager.Instance._freeSql.Insert<DbGuildMember>(new DbGuildMember()
                {
                    userName = senderName,
                    guildName = guild.guildName
                }).ExecuteAffrows();
                Log.Information($"[GuildService] 加入成员列表");
            }
            else
            {
                //如果已经有该玩家的申请就不处理
                if (guild.applicationList.Find(a => a == senderName) != null) return null;
                guild.applicationList.Add(senderName);
                MysqlManager.Instance._freeSql.Insert<DbGuildApplication>(new DbGuildApplication
                {
                    senderName = senderName,
                    guildName = guild.guildName
                }).ExecuteAffrows();
                Log.Information($"[GuildService] 加入申请列表");
            }
            return guild;
        }

        public Guild AgreeEnterGuild(string targetName, string guildName)
        {
            var guild = GetGuildByName(guildName);
            if (guild == null) return null;
            if (!guild.applicationList.Contains(targetName)) return null;

            guild.count++;
            guild.applicationList.Remove(targetName);
            guild.memberList.Add(targetName);
            return guild;
        }

        //会员退出公会
        public Guild? ExitGuild(string senderName, string guildName)
        {
            lock (_guildDictionary)
            {
                var guild = GetGuildByName(guildName);
                if (guild == null || !guild.memberList.Contains(senderName)) return null;

                guild.count--;
                guild.memberList.Remove(guildName);
                return guild;
            }
        }
    }
}
