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

        //公会聊天记录
        private Dictionary<string, List<ChatMessage>> _guildMessageDict = new();

        private int _maxMessageCount = 10;

        private ChatManager() { }

        public void Start()
        {
            LoadFriendMessage();
            LoadGuildMessage();
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

        //外部获取接口--获取指定公会的聊天记录
        public List<ChatResponse>? GetGuildChatMessage(string guildName)
        {
            if(_guildMessageDict.TryGetValue(guildName, out var list))
            {
                List<ChatResponse> res = new();
                foreach(var message in list)
                {
                    res.Add(new ChatResponse()
                    {
                        ChatType = ChatType.Guild,
                        SenderName = message.senderName,
                        Context = message.context,
                        SendTime = message.sendTime
                    });
                }
                return res;
            }
            return null;
        }

        //导入好友聊天数据
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

        //导入公会聊天记录
        private void LoadGuildMessage()
        {
            List<DbGuildMessage> list = MysqlManager.Instance._freeSql.Select<DbGuildMessage>().ToList();
            foreach(var dbMessage in list)
            {
                string guildName = dbMessage.guildName;
                ChatMessage chatMessage = new ChatMessage()
                {
                    chatType = ChatType.Guild,
                    senderName = dbMessage.senderName,
                    context = dbMessage.context,
                    sendTime = dbMessage.sendTime
                };
                if (_guildMessageDict.TryGetValue(guildName, out var guildMessageList))
                {
                    if(guildMessageList.Count >= _maxMessageCount)
                    {
                        //删除第一条
                        var firstMessage = guildMessageList.First();
                        guildMessageList.Remove(firstMessage);
                    }
                    guildMessageList.Add(chatMessage);
                }
                else
                {
                    _guildMessageDict[guildName] = new();
                    _guildMessageDict[guildName].Add(chatMessage);
                }
            }
        }

        //收到聊天消息
        public void OnReceiveChatMessage(ChatMessage chatMessage)
        {
            switch (chatMessage.chatType)
            {
                //世界聊天
                case ChatType.World:
                    lock (_worldChatMessageList)
                    {
                        _worldChatMessageList.Add(chatMessage);
                    }
                    break;

                //私聊 发送者和接收者都要存储
                case ChatType.Private:
                    InsertPrivateMessage(chatMessage.senderName, chatMessage.targetName, chatMessage);
                    InsertPrivateMessage(chatMessage.targetName, chatMessage.senderName, chatMessage);
                    break;

                //公会聊天
                case ChatType.Guild:
                    lock (_guildMessageDict)
                    {
                        if (_guildMessageDict.TryGetValue(chatMessage.targetName, out var list))
                        {
                            list.Add(chatMessage);
                        }
                        else
                        {
                            _guildMessageDict.Add(chatMessage.targetName, new());
                            _guildMessageDict[chatMessage.targetName].Add(chatMessage);
                        }
                    }
                    break;
            }
        }

        private void InsertPrivateMessage(string senderName, string targetName, ChatMessage chatMessage)
        {
            lock (_userChatMessageDict)
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
}
