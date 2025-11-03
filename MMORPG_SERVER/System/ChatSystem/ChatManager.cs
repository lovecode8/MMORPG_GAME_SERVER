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

        //玩家私聊聊天记录 玩家名对应聊天记录
        private Dictionary<string, List<ChatMessage>> _userChatMessageDict = new();

        private ChatManager() { }

        public void OnReceiveChatMessage(ChatMessage chatMessage)
        {
            switch (chatMessage.chatType)
            {
                //世界聊天
                case ChatType.World:
                    _worldChatMessageList.Add(chatMessage);
                    break;

                //私聊 发送者和接收者都要存储
                case ChatType.Private:
                    InsertPrivateMessage(chatMessage.senderName, chatMessage);
                    InsertPrivateMessage(chatMessage.targetName, chatMessage);
                    break;
            }
        }

        private void InsertPrivateMessage(string targetName, ChatMessage chatMessage)
        {
            if (_userChatMessageDict.TryGetValue(targetName, out var list))
            {
                list.Add(chatMessage);
            }
            else
            {
                _userChatMessageDict.Add(targetName, new());
                _userChatMessageDict[targetName].Add(chatMessage);
            }
        }
    }
}
