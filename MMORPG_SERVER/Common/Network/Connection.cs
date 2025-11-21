using Google.Protobuf;
using Serilog;
using System.Diagnostics;
using System.Net.Sockets;

namespace MMORPG_SERVER.Common.Network
{
    //接收消息参数
    public class PacketReceivedEventArgs : EventArgs
    {
        public Packet packet;

        public PacketReceivedEventArgs(Packet p)
        {
            packet = p;
        }
    }

    //出现警告参数
    public class WarnningOccurEventArgs : EventArgs
    {
        public string description;

        public WarnningOccurEventArgs(string d)
        {
            description = d;
        }
    }

    //出现错误参数
    public class ErrorOccutEventArgs : EventArgs
    {
        public Exception exception;

        public ErrorOccutEventArgs(Exception ex)
        {
            exception = ex;
        }
    }

    //连接关闭参数
    public class ConnentionClosedEventArgs : EventArgs
    {
        public bool isManual;

        public ConnentionClosedEventArgs(bool m)
        {
            isManual = m;
        }
    }

    //连接基类
    public class Connection
    {
        public Socket _socket;

        //委托：public delegate void EventHandler(object? sender, EventArgs e);

        public EventHandler<ConnentionClosedEventArgs>? ConnentionClosed;

        public EventHandler<PacketReceivedEventArgs>? PacketReceived;

        public EventHandler<WarnningOccurEventArgs>? WarnningOccur;

        public EventHandler<ErrorOccutEventArgs>? ErrorOccur;

        //是否主动关闭连接
        public bool? _isCloseByManual;

        public Connection(Socket socket)
        {
            _socket = socket;
        }

        public async Task StartAsync()
        {
            await ReceiveLoop();
        }

        private async Task ReceiveLoop()
        {
            try
            {
                while (_socket.Connected)
                {
                    if (_socket.Available > 0)
                    {
                        //接收消息长度、id、消息体
                        var messageLength = await _socket.ReadInt32Async();
                        var messageId = await _socket.ReadInt32Async();
                        var buffer = await _socket.ReadAsync(messageLength);
                        PacketReceived?.Invoke(this, new PacketReceivedEventArgs
                            (new Packet(messageId, buffer)));
                    }
                }
            }

            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public void SendAsync(IMessage message)
        {
            if (!_socket.Connected)
            {
                WarnningOccur?.Invoke(this, new WarnningOccurEventArgs("尝试向已经关闭的对象发送数据"));
                Close();
                return;
            }

            try
            {
                Log.Information(message.GetType().ToString());
                Packet packet = new Packet(message);
                byte[] buffer = packet.Pack();

                _socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, ar =>
                {
                    var res = _socket.EndSend(ar);
                    Debug.Assert(res > 0);
                }, null);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void HandleError(Exception ex)
        {
            if (ex is SocketException ex1)
            {
                if (ex1.SocketErrorCode != SocketError.Success)
                {
                    if (_isCloseByManual == true)
                    {
                        return;
                    }

                    //被动关闭连接
                    _isCloseByManual = false;
                    Close();
                    return;
                }

                ErrorOccur?.Invoke(this, new ErrorOccutEventArgs(ex));
            }
        }

        //关闭连接
        public void Close()
        {
            if (!_socket.Connected)
            {
                if (_isCloseByManual == false)
                {
                    ConnentionClosed?.Invoke(this, new ConnentionClosedEventArgs(false));
                }
                else if (_isCloseByManual == true)
                {
                    WarnningOccur?.Invoke(this, new WarnningOccurEventArgs("尝试重复关闭同一个连接"));
                }
                else
                {
                    WarnningOccur?.Invoke(this, new WarnningOccurEventArgs("尝试关闭一个已经关闭的连接"));
                }
                return;
            }

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                ErrorOccur?.Invoke(this, new ErrorOccutEventArgs(ex));
            }
            finally
            {
                _isCloseByManual ??= true;
                _socket.Close();
                ConnentionClosed?.Invoke(this, new ConnentionClosedEventArgs(true));
            }
        }
    }
}
