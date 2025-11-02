using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Common.Network
{
    public class ServiceBase<T> where T : ServiceBase<T>, new()
    {
        private static T? _instance;

        public static T Instance => _instance ??= new T();

        public Dictionary<Type, MethodInfo> _handlers = new Dictionary<Type, MethodInfo>();

        protected ServiceBase()
        {
            _handlers = (from m in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                         where m.Name == "OnHandle"
                         select m).ToDictionary(m => m.GetParameters()[1].ParameterType, m => m);
        }

        public bool HandleMessage<TConnection>(TConnection sender, IMessage message) where TConnection : Connection
        {
            var type = message.GetType();
            if (_handlers.ContainsKey(type))
            {
                _handlers[type].Invoke(this, new object[] { sender, message });
                return true;
            }
            return false;
        }
    }
}
