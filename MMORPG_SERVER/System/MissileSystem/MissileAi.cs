using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.System.EffectSystem;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.FightSystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.PlayerSystem;
using Serilog;
using System.Numerics;
using ZstdSharp.Unsafe;

namespace MMORPG_SERVER.System.MissileSystem
{
    public class MissileAi : Entity
    {
        private Entity? _chaseTarget;

        private Vector3 _destination;

        private float? _interactDistance;

        private float _moveSpeed;

        private Vector3 _target;

        private Player _owner;

        public MissileAi
            (int entityId, 
            EntityType entityType, 
            UnitDefine unitDefine, 
            Vector3 pos, 
            float dir,
            Entity? chaseTarget,
            Vector3? destination,
            float moveSpeed,
            Player owner,
            float? interactDistance) : base(entityId, entityType, unitDefine, pos, dir)
        {
            _chaseTarget = chaseTarget ?? null;
            _destination = destination ?? Vector3.Zero;
            _moveSpeed = moveSpeed;
            _owner = owner;
            _interactDistance = interactDistance;
        }

        public void Update()
        {
            ChaseTarget();
        }

        private void ChaseTarget()
        {
            if(_chaseTarget == null)
            {
                _target = _destination;
            }
            else
            {
                _target = _chaseTarget._position;
            }

            var distanceToTarget = Vector3.Distance(_position, _target);

            if(distanceToTarget > _interactDistance)
            {
                var direction = _target - _position;

                var moveDelta = 
                    Vector3.Normalize(direction) * _moveSpeed * MMORPG_SERVER.Time.Timer.deltaTime;

                _position += moveDelta;
            }
            else
            {
                //触发（追踪弹）
                if(_chaseTarget != null)
                {
                    Interact();
                }
                //（范围弹）
                else
                {

                }
            }
        }

        //触发 -- 对目标造成伤害、删除实体、产生特效
        private void Interact()
        {
            Log.Information("[MissileAi]导弹爆炸");
            //广播删除实体
            MissileManager.Instance.RemoveMissile(this);

            //广播产生特效
            Effect effect = new Effect
                (EntityManager.Instance.NewEntityId(),
                EntityType.Effect,
                _unitDefine, //传入导弹自己的Define
                _position,
                0,
                1.5f);
            EffectManager.Instance.AddEffect(effect);

            int demage = FightManager.Instance.GetSkillHurt(_owner, _chaseTarget);

            //广播受伤消息
            PlayerManager.Instance.Broadcast(new PlayerAttackResponse()
            {
                IsSuccessfulAttack = true,
                Damage = demage,
                PlayerId = -1,
                IsHit = true,
                TargetId = _chaseTarget._entityId
            }, this);
        }
    }
}
