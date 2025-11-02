using Google.Protobuf;
using Google.Protobuf.Reflection;
using MMORPG_SERVER.Common.Tool;
using System.Diagnostics;

namespace MMORPG_SERVER.Common.Network
{
    //网络传输消息包
    public class Packet
    {
        public int _messageID;

        public byte[] _data;

        private IMessage _message;

        public IMessage Message
        {
            get
            {
                if (_message == null)
                {
                    var type = PacketManager.MessageIdToType(_messageID);
                    var desc = type.GetProperty("Descriptor")?.GetValue(null) as MessageDescriptor;
                    Debug.Assert(desc != null);
                    _message = desc.Parser.ParseFrom(_data);
                }
                return _message;
            }
        }

        //发送消息时的构造函数
        public Packet(IMessage message)
        {
            _messageID = PacketManager.MessageTypeToID(message.GetType());
            _message = message;
            _data = message.ToByteArray();
        }

        //接收消息的构造函数
        public Packet(int messageID, byte[] data)
        {
            _messageID = messageID;
            _data = data;
        }

        //打包消息，用于传输
        public byte[] Pack()
        {
            var res = new byte[NetConfig.PacketHeaderSize + _data.Length];
            var lengthBytes = BitConverter.GetBytes(_data.Length);
            var messageIdBytes = BitConverter.GetBytes(_messageID);
            Array.Copy(lengthBytes, res, 4);
            Array.Copy(messageIdBytes, 0, res, 4, 4);
            Array.Copy(_data, 0, res, 8, _data.Length);
            return res;
        }
    }
}
