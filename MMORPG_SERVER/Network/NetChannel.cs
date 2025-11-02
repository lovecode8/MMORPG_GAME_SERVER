using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.System.UserSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Network
{
    //与客户端通信的通道
    public class NetChannel : Connection
    {
        public User _user;

        public long _lastActiveTime;

        public LinkedListNode<NetChannel>? _linkedListNode;

        public NetChannel(Socket socket) : base(socket)
        {
            ConnentionClosed += OnConnectionClosed;
            WarnningOccur += OnWarnningOccur;
            ErrorOccur += OnErrorOccur;
        }

        public void SetUser(User user)
        {
            _user = user;
        }

        public void OnConnectionClosed(object? sender, ConnentionClosedEventArgs args)
        {
            if (args.isManual)
            {
                Log.Information($"[channel:{this}] 主动断开连接");
            }
            else
            {
                Log.Information($"[channel:{this}] 被动断开连接");
            }
        }

        public void OnWarnningOccur(object? sender, WarnningOccurEventArgs args)
        {
            Log.Warning($"[channel:{this}] 出现警告：{args.description}");
        }

        public void OnErrorOccur(object? sender, ErrorOccutEventArgs args)
        {
            Log.Warning($"[channel:{this}] 出现错误：{args.exception.Message}");
        }
    }
}
