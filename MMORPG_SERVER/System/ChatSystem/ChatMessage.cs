using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.ChatSystem
{
    public class ChatMessage
    {
        public ChatType chatType;

        public string context;

        public int senderId;

        public string senderName;

        //玩家、公会?
        public int? targetId;
    }
}
