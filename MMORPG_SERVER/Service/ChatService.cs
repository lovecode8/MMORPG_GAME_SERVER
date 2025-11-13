using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.ChatSystem;
using MMORPG_SERVER.System.GuildSystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.System.UserSystem;
using Org.BouncyCastle.Asn1.Crmf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Service
{
    public class ChatService : ServiceBase<ChatService>
    {
        //处理收到聊天请求消息
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
                        OnReceiveWorldMessage(chatRequest, channel);
                        break;

                    //私聊
                    case ChatType.Private:
                        OnReceiveFriendMessage(chatRequest, channel);
                        break;

                    //公会
                    case ChatType.Guild:
                        OnReceiveGuildMessage(chatRequest, channel);
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

        private void OnReceiveWorldMessage(ChatRequest chatRequest, NetChannel? channel)
        {
            PlayerManager.Instance.Broadcast(new ChatResponse()
            {
                ChatType = chatRequest.ChatType,
                Context = chatRequest.Context,
                SenderName = channel?._user._dbUser.UserName
            }, channel._user._player);
        }

        private void OnReceiveFriendMessage(ChatRequest chatRequest, NetChannel? channel)
        {
            //存入数据库
            MysqlManager.Instance._freeSql.Insert<DbFriendMessage>(new DbFriendMessage()
            {
                senderName = channel._user._dbUser.UserName,
                targetName = chatRequest.TargetName,
                context = chatRequest.Context,
                sendTime = chatRequest.SendTime
            }).ExecuteAffrows();

            var user = UserManager.Instance.GetUserByName(chatRequest.TargetName);

            //目标用户在线就发给他
            user?._netChannel.SendAsync(new ChatResponse()
            {
                ChatType = ChatType.Private,
                Context = chatRequest.Context,
                SenderName = channel._user._dbUser.UserName,
                SendTime = chatRequest.SendTime
            });
        }

        private void OnReceiveGuildMessage(ChatRequest chatRequest, NetChannel? channel)
        {
            string senderName = channel._user._dbUser.UserName;
            
            //存入数据库
            MysqlManager.Instance._freeSql.Insert<DbGuildMessage>(new DbGuildMessage()
            {
                senderName = senderName,
                context = chatRequest.Context,
                sendTime = chatRequest.SendTime,
                guildName = chatRequest.TargetName
            }).ExecuteAffrows();

            var guild = GuildManager.Instance.GetGuildByName(chatRequest.TargetName);

            //发送给所有同会在线成员
            if(guild != null)
            {
                foreach(string userName in guild.memberList)
                {
                    var user = UserManager.Instance.GetUserByName(userName);
                    if (userName == senderName) continue;
                    user?._netChannel.SendAsync(new ChatResponse()
                    {
                        ChatType = ChatType.Guild,
                        SenderName = senderName,
                        Context = chatRequest.Context,
                        SendTime = chatRequest.SendTime,
                        TargetName = chatRequest.TargetName
                    });
                }
            }
        }
    }
}
