using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "FriendApplication")]
    public class DbFriendApplication
    {
        public string senderName { get; set; }
        public string targetName { get; set; }
    }
}
