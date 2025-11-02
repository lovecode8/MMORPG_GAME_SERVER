using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Tool
{
    public static class SingletonCreator
    {
        public static T CreateSingleton<T>() where T : class
        {
            var type = typeof(T);

            //获取私有函数
            var ctorInfos = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
            var ctor = Array.Find(ctorInfos, c => c.GetParameters().Length == 0) ??
                throw new Exception($"单例{type}必须有一个私有无参构造函数");
            var publicCtors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            if (publicCtors.Length > 0)
            {
                throw new Exception($"单例{type}不可以包含公有构造函数");
            }

            T? instance = ctor.Invoke(null) as T;
            Debug.Assert(instance != null);
            return instance;
        }
    }
    //单例基类
    public abstract class Singleton<T> where T : Singleton<T>
    {
        private static T? _instance;

        public static T Instance
        {
            get
            {
                _instance ??= SingletonCreator.CreateSingleton<T>();
                return _instance;
            }
        }

        public virtual void Dispose()
        {
            _instance = null;
        }
    }
}
