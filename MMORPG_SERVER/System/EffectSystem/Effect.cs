using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.System.EntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.EffectSystem
{
    public class Effect : Entity
    {
        private float _destoryInterval;

        private float _timer;

        public void Update()
        {
            _timer += MMORPG_SERVER.Time.Timer.deltaTime;

            if(_timer > _destoryInterval)
            {
                EffectManager.Instance.RemoveEffect(this);
            }
        }

        public Effect(int entityId, 
            EntityType entityType, 
            UnitDefine unitDefine, 
            Vector3 pos, 
            float dir,
            float destoryInterval) : base(entityId, entityType, unitDefine, pos, dir)
        {
            _destoryInterval = destoryInterval;
        }
    }
}
