using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
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
                EntityManager.Instance.OnReceiveEntitySyncRequest(entitySyncRequest);
            });
        }

    }
}
