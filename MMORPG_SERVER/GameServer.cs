using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.Service;
using MMORPG_SERVER.Time;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER
{
    //游戏服务器主类
    public class GameServer
    {
        private Socket _serverSocket;

        private LinkedList<NetChannel> _channels;

        private TimeWheel _connectionCleanupTimer;

        private int _cleanUpTime = 10000;

        public GameServer(int port)
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            _channels = new();
            _connectionCleanupTimer = new TimeWheel(1000);
            Task.Run(() =>
            {
                _ = _connectionCleanupTimer.Start();
            });
        }

        public async Task Run()
        {
            Log.Information($"[server] 服务器已开启");

            await UpdateManager.Instance.Start();
            _serverSocket.Listen();

            Log.Information($"[server] 等待客户端连接......");
            while (true)
            {
                var socket = await _serverSocket.AcceptAsync();
                Log.Information($"[server] 客户端连接：{socket.RemoteEndPoint}");
                NetChannel channel = new NetChannel(socket);
                OnNewChannelConnect(channel);
                _ = Task.Run(channel.StartAsync);
            }
        }

        //接收到新连接
        private void OnNewChannelConnect(NetChannel channel)
        {
            lock (_channels)
            {
                var node = _channels.AddLast(channel);
                channel._linkedListNode = node;
            }
            channel.ConnentionClosed += OnConnentionClosed;
            channel.PacketReceived += OnPacketReceived;
            channel._lastActiveTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

            _connectionCleanupTimer.AddTask(_cleanUpTime / 10, (task) =>
            {
                var now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                var duration = now - channel._lastActiveTime;

                if (duration > _cleanUpTime)
                {
                    Log.Information($"接收 {channel._socket.RemoteEndPoint} 的消息超时，已关闭与它的连接");
                    channel.Close();
                }
                else
                {
                    _connectionCleanupTimer.AddTask(_cleanUpTime / 10, task.action);
                }
            });
        }

        private void OnConnentionClosed(object? sender, ConnentionClosedEventArgs args)
        {
            var channel = sender as NetChannel;
            Debug.Assert(channel != null);

            //关闭它的所有服务
            UserService.Instance.OnConnectionClosed(channel);
            PlayerService.Instance.OnConectionClosed(channel);
            
            lock (_channels)
            {
                if (channel._linkedListNode != null)
                {
                    _channels.Remove(channel._linkedListNode);
                }
            }
        }

        private void OnPacketReceived(object? sender, PacketReceivedEventArgs args)
        {
            var channel = sender as NetChannel;
            Debug.Assert(channel != null);

            channel._lastActiveTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

            //各模块处理消息
            //Log.Information(args.packet.Message.GetType().ToString());
            UserService.Instance.HandleMessage(channel, args.packet.Message);
            CharacterService.Instance.HandleMessage(channel, args.packet.Message);
            PlayerService.Instance.HandleMessage(channel, args.packet.Message);
            ChatService.Instance.HandleMessage(channel, args.packet.Message);
            FriendService.Instance.HandleMessage(channel, args.packet.Message);
            GuildService.Instance.HandleMessage(channel, args.packet.Message);
            EntityService.Instance.HandleMessage(channel, args.packet.Message);
            FightService.Instance.HandleMessage(channel, args.packet.Message);
            InventoryService.Instance.HandleMessage(channel, args.packet.Message);
            ShopService.Instance.HandleMessage(channel, args.packet.Message);
            TaskService.Instance.HandleMessage(channel, args.packet.Message);
        }
    }
}
