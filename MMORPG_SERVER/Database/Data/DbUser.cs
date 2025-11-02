using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    //对应数据库的User表
    [Table(Name = "User")]
    public class DBUser
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int UserId { get; set; }

        public string? UserName { get; set; }
        public string? Password { get; set; }

        public DBUser(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
