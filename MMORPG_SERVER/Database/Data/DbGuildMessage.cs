using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "GuildMessage")]
    public class DbGuildMessage
    {
        public string senderName { get; set; }

        public string context { get; set; }

        public long sendTime { get; set; }

        public string guildName { get; set; }
    }
}
