using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "Guild")]
    public class DbGuild
    {
        public string guildName { get; set; }

        public string ownerName { get; set; }

        public int count { get; set; }

        public string slogan { get; set; }

        public int iconIndex { get; set; }

        //入会审核
        public int needEnterCheck { get; set; }
    }
}
