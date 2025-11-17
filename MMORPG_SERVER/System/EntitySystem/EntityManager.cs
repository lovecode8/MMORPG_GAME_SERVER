using MMORPG_SERVER.Tool;
using Serilog;


namespace MMORPG_SERVER.System.EntitySystem
{
    //实体管理器
    public class EntityManager : Singleton<EntityManager>
    {
        private Dictionary<int, Entity> _entityDictionaty = new();

        private Dictionary<int, Entity> _addQueue = new();

        private Dictionary<int, Entity> _removeQueue = new();

        private int _entityAddId = 0;

        private EntityManager() { }

        public void Update()
        {
            foreach(Entity entity in _addQueue.Values)
            {
                if (!_entityDictionaty.ContainsKey(entity._entityId))
                {
                    _entityDictionaty[entity._entityId] = entity;
                }
            }
            _addQueue.Clear();

            foreach(Entity entity in _removeQueue.Values)
            {
                if (_entityDictionaty.ContainsKey(entity._entityId))
                {
                    _entityDictionaty.Remove(entity._entityId);
                }
            }
            _removeQueue.Clear();
        }

        public int NewEntityId()
        {
            Log.Information(_entityAddId.ToString());
            return _entityAddId++;
        }

        public Entity? GetEntity(int entityId)
        {
            _entityDictionaty.TryGetValue(entityId, out var entity);
            if(entity != null)
            {
                entity = null;
            }
            else
            {
                _addQueue.TryGetValue(entityId, out entity);
            }
            return entity;
        }

        public void AddEntity(Entity entity)
        {
            lock (_entityDictionaty)
            {
                if(!_entityDictionaty.ContainsKey(entity._entityId))
                {
                    _entityDictionaty[entity._entityId] = entity;
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

        public Dictionary<int, Entity> GetEntityDict()
        {
            return _entityDictionaty;
        }
    }
}
