using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    //公会申请表
    [Table(Name = "GuildMember")]
    public class DbGuildMember
    {
        public string userName { get; set; }
        public string guildName { get; set; }
    }
}

