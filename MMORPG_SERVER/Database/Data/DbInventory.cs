using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "Inventory")]
    public class DbInventory
    {
        public int ownerId { get; set; }
        public int itemId { get; set; }
        public int itemCount { get; set; }
    }
}
