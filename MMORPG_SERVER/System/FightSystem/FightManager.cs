using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.AttributeSystem;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.MonsterSystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.Tool;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static FreeSql.Internal.GlobalFilter;

namespace MMORPG_SERVER.System.FightSystem
{
    public class FightManager : Singleton<FightManager>
    {
        private FightManager() { }

        public int GetFightHurtValue(Entity attacker, Entity target, int camboCount = 0)
        {
            //基础伤害  
            var demage = attacker._unitDefine.AttackDemage[camboCount];

            //攻击者是Player--考虑攻击力加成
            if (attacker._entityType == EntityType.Player)
            {
                var attackerAttribute = AttributeManager.Instance.
                    GetPlayerAttribute((attacker as Player)._user._userId);

                //附加伤害
                if (attackerAttribute != null)
                {
                    demage += attackerAttribute._atkAddition;
                }
            }
            
            //攻击目标是Player--考虑防御力加成
            if (target._entityType == EntityType.Player)
            {
                var targetAttribute = AttributeManager.Instance.
                    GetPlayerAttribute((target as Player)._user._userId);
                if (targetAttribute != null) demage -= targetAttribute._defAddition;
            }

            //最低伤害校准
            demage = Math.Clamp(demage, 10, 9999);

            //更新属性
            switch (target._entityType)
            {
                case EntityType.Player:
                    (target as Player).GetHurt(demage);
                    break;
                case EntityType.Monster:
                    (target as Monster).GetHurt(attacker, demage);
                    break;
            }

            return demage;
        }

        //某个Monster死亡
        public void OnMonsterDie(Entity attacker, Entity deadMonster)
        {
            //击败者增加经验值
            if(attacker is Player player)
            {
                player.AddExp(deadMonster._unitDefine.KilledExp);
            }

            //实体离开游戏
            MonsterManager.Instance.RemoveMonster(deadMonster as Monster);
            EntityManager.Instance.RemoveEntity(deadMonster);
            MapManager.Instance.EntityLeave(deadMonster);

            //生成奖励
            //在场景中创建物品
            //TODO：后续增加随机物品功能
            int itemId = 2;
            var itemDefine = DataManager.Instance.GetItemDefine(itemId);
            var entity = new Entity(EntityManager.Instance.NewEntityId(),
                EntityType.Item,
                DataManager.Instance.GetUnitDefine(itemDefine.UnitId),
                deadMonster._position,
                0);
            entity.itemId = itemId;
            EntityManager.Instance.AddEntity(entity);
            MapManager.Instance.EntityEnter(entity);
        }
    }
}
