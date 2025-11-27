using Extension;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.Time;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Numerics;


namespace MMORPG_SERVER.System.EntitySystem
{
    //实体管理器
    public class EntityManager : Singleton<EntityManager>
    {
        //线程安全
        private Dictionary<int, Entity> _entityDictionaty = new();

        private Dictionary<int, Entity> _addQueue = new();

        private Dictionary<int, Entity> _removeQueue = new();

        private int _entityAddId = 0;

        private float _syncInterval = 0.15f;

        private float _syncTimer;

        private EntityManager() { }

        public void Update()
        {
            _syncTimer += Time.Timer.deltaTime;
            if (_syncTimer >= _syncInterval)
            {
                SyncEntityToClient();
                _syncTimer = 0;
            }

            UpdateEntityDict();
        }

        private void UpdateEntityDict()
        {
            foreach (Entity entity in _addQueue.Values)
            {
                AddEntity(entity);
            }
            _addQueue.Clear();

            foreach (Entity entity in _removeQueue.Values)
            {
                RemoveEntity(entity);
            }
            _removeQueue.Clear();
        }

        //向客户端发送所有实体数据
        private void SyncEntityToClient()
        {
            if (SyncCondition())
            {
                PlayerManager.Instance.SyncAllEntityData(_entityDictionaty);
            }
        }

        public int NewEntityId()
        {
            Log.Information(_entityAddId.ToString());
            return _entityAddId++;
        }

        public Entity? GetEntity(int entityId)
        {
            lock (_entityDictionaty)
            {
                if(_entityDictionaty.TryGetValue(entityId, out var entity))
                {
                    return entity;
                }
                return null;
            }
        }

        public void AddEntity(Entity entity)
        {
            lock (_entityDictionaty)
            {
                if(!_entityDictionaty.ContainsKey(entity._entityId))
                {
                    _entityDictionaty.Add(entity._entityId, entity);
                }
            }
        }

        public void RemoveEntity(Entity entity)
        {
            lock (_entityDictionaty)
            {
                if (_entityDictionaty.ContainsKey(entity._entityId))
                {
                    _entityDictionaty.Remove(entity._entityId);
                }
            }
        }

        public void OnReceiveEntitySyncRequest(EntitySyncRequest entitySyncRequest)
        {
            var entity = GetEntity(entitySyncRequest.EntityId);
            SyncEntityData(entity, entitySyncRequest.Position.ToVector3(),
                entitySyncRequest.RotationY, entitySyncRequest.StateId);
            PlayerManager.Instance.OnPlayerMove(entity as Player);
        }

        public void SyncEntityData(Entity entity, Vector3 pos, float rotY, int stateId)
        {
            entity._position = pos;
            entity._rotationY = rotY;
            entity._stateId = stateId;
        }

        public Dictionary<int, Entity> GetEntityDict()
        {
            return _entityDictionaty;
        }

        public bool IsAttackTargetVaild(int attackerId, int targetId)
        {
            var attacker = GetEntity(attackerId);
            var target = GetEntity(targetId);
            if (attacker == null || target == null) return false;

            if (Vector3.Distance(attacker._position, target._position) > 5f) return false;

            var directionAttackerToTarget = target._position - attacker._position;
            if(!Vector3Extensions.IsInAngleRange(directionAttackerToTarget, attacker._rotationY, 60))
            {
                return false;
            }
            return true;
        }

        //同步实体的条件
        private bool SyncCondition()
        {
            int count = 0;
            foreach(var entity in _entityDictionaty.Values)
            {
                if(entity._entityType != EntityType.Item)
                {
                    count++;
                }
                if(count >= 2)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
