using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.ChatSystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.System.UserSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Service
{
    public class ChatService : ServiceBase<ChatService>
    {
        public void OnHandle(object sender, ChatRequest chatRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                Log.Information($"[ChatService] 收到用户聊天消息：{chatRequest.Context}");
                NetChannel? channel = sender as NetChannel;

                //世界、私聊、公会
                switch (chatRequest.ChatType)
                {
                    //世界聊天
                    case ChatType.World:
                        PlayerManager.Instance.Broadcast(new ChatResponse()
                        {
                            ChatType = chatRequest.ChatType,
                            Context = chatRequest.Context,
                            SenderName = channel?._user._dbUser.UserName
                        }, channel._user._player);
                        break;

                    //私聊
                    case ChatType.Private:
                        //存入数据库
                        MysqlManager.Instance._freeSql.Insert<DbFriendMessage>
                        (new DbFriendMessage()
                        {
                            senderName = channel?._user._dbUser.UserName,
                            targetName = chatRequest.TargetName,
                            context = chatRequest.Context,
                            sendTime = chatRequest.SendTime
                        }).ExecuteAffrows();

                        User? user = UserManager.Instance.GetUserByName(chatRequest.TargetName);
                        //目标用户在线就发给他
                        if(user != null)
                        {
                            user._netChannel.SendAsync(new ChatResponse()
                            {
                                ChatType = ChatType.Private,
                                Context = chatRequest.Context,
                                SenderName = channel._user._dbUser.UserName,
                                SendTime = chatRequest.SendTime
                            });
                        }
                        break;
                }

                ChatManager.Instance.OnReceiveChatMessage(new ChatMessage()
                {
                    chatType = chatRequest.ChatType,
                    context = chatRequest.Context,
                    senderName = channel._user._dbUser.UserName,
                    targetName = chatRequest.TargetName ?? null,
                    sendTime = chatRequest.SendTime
                });
            });
        }
    }
}
