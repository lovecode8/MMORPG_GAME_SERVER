using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Common.Network
{
    //网络连接常量
    public class NetConfig
    {
        public static string host = "127.0.0.1";

        public static int port = 8080;

        //数据包的头部大小
        public static int PacketHeaderSize = 8;
    }
}
