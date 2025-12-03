using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "Character")]
    public class DbCharacter
    {
        //常量
        public int UserId { get; set; }
        public int UnitId { get; set; }
        public string Name { get; set; }

        //变量
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int MaxHpAddition { get; set; }
        public int MaxMpAddition { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
        public float rotX { get; set; }
        public float rotY { get; set; }
        public float rotZ { get; set; }
        public int Level { get; set; }
        public int Gold { get; set; }
        public int Exp { get; set; }
    }
}
