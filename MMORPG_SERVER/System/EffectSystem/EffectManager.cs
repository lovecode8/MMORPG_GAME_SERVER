using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.Tool;
using Serilog;

namespace MMORPG_SERVER.System.EffectSystem
{
    public class EffectManager : Singleton<EffectManager>
    {
        private EffectManager() { }

        private Dictionary<int, Effect> _effectDict = new();

        public void Update()
        {
            foreach(var effect in _effectDict.Values)
            {
                effect.Update();
            }
        }

        public void AddEffect(Effect effect)
        {
            Log.Information("[EffectManager]生成特效");
            _effectDict[effect._entityId] = effect;

            EntityManager.Instance.AddEntity(effect);
            MapManager.Instance.EntityEnter(effect);
        }

        public void RemoveEffect(Effect effect)
        {
            if (_effectDict.ContainsKey(effect._entityId))
            {
                Log.Information("[EffectManager]删除特效");
                _effectDict.Remove(effect._entityId);

                EntityManager.Instance.RemoveEntity(effect);
                MapManager.Instance.EntityLeave(effect);
            }
        }
    }
}
