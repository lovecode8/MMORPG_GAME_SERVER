using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "Equip")]
    public class DbEquip
    {
        public int ownerId { get; set; }
        public int gridIndex { get; set; }
        public int itemId { get; set; }
    }
}
