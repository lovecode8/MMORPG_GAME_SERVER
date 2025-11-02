using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.ChatSystem;
using MMORPG_SERVER.System.PlayerSystem;
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
                    case ChatType.World:
                        PlayerManager.Instance.Broadcast(new ChatResponse()
                        {
                            ChatType = chatRequest.ChatType,
                            Context = chatRequest.Context,
                            SenderName = channel?._user._dbUser.UserName
                        }, channel._user._player);
                        break;
                }
                ChatManager.Instance.OnReceiveChatMessage(new ChatMessage()
                {
                    chatType = chatRequest.ChatType,
                    context = chatRequest.Context,
                    senderId = channel._user._userId,
                    senderName = channel?._user._dbUser.UserName,
                });
            });
        }
    }
}
