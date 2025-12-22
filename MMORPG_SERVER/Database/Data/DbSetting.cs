
using FreeSql.DataAnnotations;

namespace MMORPG_SERVER.Database.Data
{
    [Table(Name = "Setting")]
    public class DbSetting
    {
        public int OwnerId { get; set; }
        public short IsMusicPlay { get; set; }
        public short IsEffectPlay { get; set; }
        public int MusicVolume { get; set; }
        public int EffectVolume { get; set; }
    }
}
