using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.GuildSystem
{
    //服务器公会类
    public class Guild
    {
        public string guildName;

        public string ownerName;

        public int count;

        public string slogan;

        public int iconIndex;

        //入会审核
        public bool needEnterCheck;

        //申请列表
        public List<string> applicationList;

        //成员列表（不含会长）
        public List<string> memberList;
    }
}
