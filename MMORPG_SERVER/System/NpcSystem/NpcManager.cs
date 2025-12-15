using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.MonsterSystem;
using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.NpcSystem
{
    //场景中的Npc管理器
    public class NpcManager : Singleton<NpcManager>
    {
        private NpcManager() { }

        private Dictionary<int, Npc> _npcDict = new();

        private float _timer;

        public void Start()
        {
            AddFirstNpc();
        }

        public void Update()
        {
            foreach(var npc in _npcDict.Values)
            {
                npc.Update();
            }
        }

        //生成第一个Npc
        private void AddFirstNpc()
        {
            var unitDefine = DataManager.Instance.GetUnitDefine(12);
            var movePosList = new List<Vector3>();
            //导入移动数据
            foreach (var pos in unitDefine.MovePosition)
            {
                movePosList.Add(pos.ToVector3());
            }
            var npc = new Npc
                (EntityManager.Instance.NewEntityId(),
                EntityType.NPC,
                unitDefine,
                unitDefine.OriginalPosition.ToVector3(),
                0,
                movePosList);
            var controller = new NpcAi(npc);
            npc.SetController(controller);
            controller.Start();
            AddNpc(npc);
        }

        public void AddNpc(Npc npc)
        {
            if(!_npcDict.ContainsKey(npc._entityId))
            {
                _npcDict.Add(npc._entityId, npc);
                EntityManager.Instance.AddEntity(npc);
                MapManager.Instance.EntityEnter(npc);
            }
        }

        public void RemoveNpc(Npc npc)
        {
            if (_npcDict.ContainsKey(npc._entityId))
            {
                _npcDict.Remove(npc._entityId);
                EntityManager.Instance.RemoveEntity(npc);
                MapManager.Instance.EntityLeave(npc);
            }
        }
    }
}
