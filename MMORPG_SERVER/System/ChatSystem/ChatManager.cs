using Extension;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private Dictionary<string, Dictionary<string, List<ChatMessage>>> _userChatMessageDict = new();

        private ChatManager() { }

        public void Start()
        {
            LoadFriendMessage();
        }

        //外部获取接口--获取指定玩家和所有人的聊天记录
        public Dictionary<string, ChatMessageList> GetFriendChatMessageDict(string userName)
        {
            if (_userChatMessageDict.TryGetValue(userName, out var dict) && dict.Count > 0)
            {
                Dictionary<string, ChatMessageList> res = new();
                foreach (var kv in dict)
                {
                    res.Add(kv.Key, kv.Value.ToChatMessageList());
                }
                return res;
            }
            return null;
        }

        //导入数据
        private void LoadFriendMessage()
        {
            List<DbFriendMessage> list = MysqlManager.Instance._freeSql.Select<DbFriendMessage>().ToList();
            foreach(DbFriendMessage message in list)
            {
                ChatMessage chatMessage = new ChatMessage()
                {
                    chatType = ChatType.Private,
                    senderName = message.senderName,
                    targetName = message.targetName,
                    context = message.context,
                    sendTime = message.sendTime
                };
                InsertPrivateMessage(chatMessage.senderName, chatMessage.targetName, chatMessage);
                InsertPrivateMessage(chatMessage.targetName, chatMessage.senderName, chatMessage);
            }
        }

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
                    InsertPrivateMessage(chatMessage.senderName, chatMessage.targetName, chatMessage);
                    InsertPrivateMessage(chatMessage.targetName, chatMessage.senderName, chatMessage);
                    break;
            }
        }

        private void InsertPrivateMessage(string senderName, string targetName, ChatMessage chatMessage)
        {
            if (_userChatMessageDict.TryGetValue(senderName, out var dict))
            {
                if (_userChatMessageDict[senderName].TryGetValue(targetName, out var list))
                {
                    list.Add(chatMessage);
                }
                else
                {
                    dict.Add(targetName, new());
                    dict[targetName].Add(chatMessage);
                }
            }
            else
            {
                _userChatMessageDict.Add(senderName, new());
                _userChatMessageDict[senderName].Add(targetName, new());
                _userChatMessageDict[senderName][targetName].Add(chatMessage);
            }
        }
    }
}
