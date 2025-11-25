using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "Attribute")]
    public class DbAttribute
    {
        public int ownerId { get; set; }
        public int atkAddition { get; set; }
        public int defAddition { get; set; }
    }
}
