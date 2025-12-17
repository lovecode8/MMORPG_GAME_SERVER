using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.MissileSystem
{
    //追踪导弹管理器
    public class MissileManager : Singleton<MissileManager>
    {
        private MissileManager() { }

        private Dictionary<int, MissileAi> _missileDict = new();

        public void Update()
        {
            foreach(var missile in _missileDict.Values)
            {
                missile.Update();
            }
        }

        public void AddMissile(MissileAi missile)
        {
            Log.Information(missile._position.ToString());
            _missileDict.Add(missile._entityId, missile);

            EntityManager.Instance.AddEntity(missile);
            MapManager.Instance.EntityEnter(missile);
        }

        public void RemoveMissile(MissileAi missile)
        {
            _missileDict.Remove(missile._entityId);

            EntityManager.Instance.RemoveEntity(missile);
            MapManager.Instance.EntityLeave(missile);
        }
    }
}
