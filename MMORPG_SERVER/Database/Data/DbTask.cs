using FreeSql.DataAnnotations;


namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "Task")]
    public class DbTask
    {
        public int taskId { get; set; }
        public int ownerId { get; set; }
        public int currentCount { get; set; }
    }
}
