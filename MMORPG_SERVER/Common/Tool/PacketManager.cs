using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Common.Tool
{
    public class PacketManager
    {
        public static IEnumerable<Type> allMessageType = new List<Type>();

        public static readonly Type[] sortedType;

        static PacketManager()
        {
            allMessageType = from t in Assembly.GetExecutingAssembly().GetTypes()
                             where typeof(IMessage).IsAssignableFrom(t)
                             select t;

            sortedType = allMessageType.OrderBy(t => t.Name).ToArray();
        }

        public static Type MessageIdToType(int messageID)
        {
            return sortedType[messageID];
        }

        public static int MessageTypeToID(Type type)
        {
            return Array.IndexOf(sortedType, type);
        }
    }
}
