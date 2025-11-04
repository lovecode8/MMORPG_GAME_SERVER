

using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "Friend")]
    public class DbFriend
    {
        public string name1 { get; set; }
        public string name2 { get; set; }
    }
}
