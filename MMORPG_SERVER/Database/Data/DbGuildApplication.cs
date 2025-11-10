using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    //公会申请表
    [Table(Name = "GuildApplication")]
    public class DbGuildApplication
    {
        public string senderName { get; set; }
        public string guildName { get; set; }
    }
}
