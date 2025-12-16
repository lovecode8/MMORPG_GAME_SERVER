

using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.System.ChatSystem;
using MMORPG_SERVER.System.FriendSystem;
using MMORPG_SERVER.System.GuildSystem;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Extension
{
    public static class ProtoEntension
    {
        //NetVector3转Vector3
        public static Vector3 ToVector3(this NetVector3 netVector3)
        {
            return new Vector3(netVector3.X, netVector3.Y, netVector3.Z);
        }

        public static NetVector3 ToNetVector3(this Vector3 vector3)
        {
            return new NetVector3() { X = vector3.X, Y = vector3.Y, Z = vector3.Z };
        }

        public static ChatMessageList ToChatMessageList(this List<ChatMessage> list)
        {
            ChatMessageList messageList = new();
            foreach (ChatMessage chatMessage in list)
            {
                messageList.List.Add(new ChatResponse
                {
                    ChatType = chatMessage.chatType,
                    SenderName = chatMessage.senderName,
                    Context = chatMessage.context,
                    SendTime = chatMessage.sendTime,
                    TargetName = chatMessage.targetName
                });
            }
            return messageList;
        }

        //GuildInfo转Guild
        public static Guild ToGuild(this GuildInfo guildInfo)
        {
            return new Guild()
            {
                guildName = guildInfo.GuildName,
                slogan = guildInfo.GuildSlogan,
                ownerName = guildInfo.OwnerName,
                count = guildInfo.Count,
                iconIndex = guildInfo.IconIndex,
                needEnterCheck = guildInfo.NeedEnterCheck,
                applicationList = new(),
                memberList = new()
            };
        }

        //Guild转GuildInfo
        public static GuildInfo ToGuildInfo(this Guild guild)
        {
            var guildInfo = new GuildInfo()
            {
                GuildName = guild.guildName,
                GuildSlogan = guild.slogan,
                OwnerName = guild.ownerName,
                Count = guild.count,
                IconIndex = guild.iconIndex,
                NeedEnterCheck = guild.needEnterCheck,
            };
            foreach(string userName in guild.applicationList)
            {
                guildInfo.ApplicationList.Add(FriendManager.Instance.GetFriendInfoByName(userName));
            }

            foreach (string userName in guild.memberList)
            {
                guildInfo.MemberList.Add(FriendManager.Instance.GetFriendInfoByName(userName));
            }
            return guildInfo;
        }

        //TaskDefine转BaseTask
        public static BaseTask ToBaseTask(this TaskDefine taskDefine)
        {
            return new BaseTask()
            {
                CurrentCount = 0,
                TargetCount = taskDefine.TargetCount,
                TaskContent = taskDefine.Content,
                TaskId = taskDefine.ID
            };
        }
    }
}
