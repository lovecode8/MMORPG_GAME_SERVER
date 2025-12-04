using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.System.EntitySystem;
using System.Numerics;

namespace MMORPG_SERVER.System.MissileSystem
{
    public class MissileAi : Entity
    {
        private Entity _chaseTarget;

        private float _interactDistance;

        public MissileAi
            (int entityId, 
            EntityType entityType, 
            UnitDefine unitDefine, 
            Vector3 pos, 
            float dir,
            Entity chaseTarget) : base(entityId, entityType, unitDefine, pos, dir)
        {
            _chaseTarget = chaseTarget;
        }

        public void Update()
        {
            ChaseTarget();
        }

        private void ChaseTarget()
        {
            var distanceToTarget = Vector3.Distance(_position, _chaseTarget._position);
        }
    }
}
