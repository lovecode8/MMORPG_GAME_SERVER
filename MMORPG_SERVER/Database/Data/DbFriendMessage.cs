using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "FriendMessage")]
    public class DbFriendMessage
    {
        public string senderName { get; set; }

        public string targetName { get; set; }

        public string context { get; set; }

        public long sendTime { get; set; }
    }
}
