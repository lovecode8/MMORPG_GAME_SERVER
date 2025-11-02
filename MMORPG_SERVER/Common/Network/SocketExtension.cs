using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Common.Network
{
    //Socket的读取扩展方法
    public static class SocketExtension
    {
        //读取指定大小的字节数组
        public static async Task<byte[]> ReadAsync(this Socket socket, int size)
        {
            Debug.Assert(size >= 0);

            if (size == 0)
            {
                return Array.Empty<byte>();
            }
            var buffer = new byte[size];
            int readBufferSize = 0;

            while (readBufferSize < size)
            {
                int currentSize = await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer, readBufferSize, size - readBufferSize),
                    SocketFlags.None);

                Debug.Assert(currentSize > 0);
                readBufferSize += currentSize;
            }

            return buffer;
        }

        //读取int值
        public static async Task<int> ReadInt32Async(this Socket socket)
        {
            var res = await socket.ReadAsync(sizeof(int));
            return BitConverter.ToInt32(res);
        }
    }
}
