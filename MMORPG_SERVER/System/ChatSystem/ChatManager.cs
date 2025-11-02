using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.ChatSystem
{
    //服务器聊天管理器
    public class ChatManager : Singleton<ChatManager>
    {
        //世界、系统聊天消息列表
        private List<ChatMessage> _worldChatMessageList = new();

        private ChatManager() { }

        public void OnReceiveChatMessage(ChatMessage chatMessage)
        {
            switch (chatMessage.chatType)
            {
                case ChatType.World:
                    _worldChatMessageList.Add(chatMessage);
                    break;
            }
        }
    }
}
