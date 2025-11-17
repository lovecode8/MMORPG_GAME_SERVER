using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Service
{
    public class EntityService : ServiceBase<EntityService>
    {
        public void OnHandle(object sender, EntitySyncRequest entitySyncRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                Log.Information($"收到实体同步位置消息：{entitySyncRequest.Transform.Position.X}");
            });
        }

    }
}
