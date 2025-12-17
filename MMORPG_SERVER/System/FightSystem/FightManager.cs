using MMORPG_SERVER.Data.CS;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.AttributeSystem;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.MonsterSystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.System.TaskSystem;
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
    //战斗管理器
    public class FightManager : Singleton<FightManager>
    {
        private FightManager() { }

        private Random _random = new();

        public int GetFightHurtValue(Entity attacker, Entity target, int camboCount = 0)
        {
            //基础伤害  
            var demage = attacker._unitDefine.AttackDemage[camboCount];

            //攻击者是Player--考虑攻击力加成
            if (attacker is Player player)
            {
                var attackerAttribute = 
                    AttributeManager.Instance.GetPlayerAttribute(player._user._userId);

                //附加伤害
                if (attackerAttribute != null)
                {
                    demage += attackerAttribute._atkAddition;
                }

                //等级伤害
                demage += player._dbCharacter.Level * 10;
            }
            
            //攻击目标是Player--考虑防御力加成
            if (target._entityType == EntityType.Player)
            {
                var targetAttribute = AttributeManager.Instance.
                    GetPlayerAttribute((target as Player)._user._userId);
                if (targetAttribute != null) demage -= targetAttribute._defAddition;
            }

            //随机浮动
            demage = (int)(demage * _random.Next(5, 7) * 0.1f);

            //最低伤害校准
            demage = Math.Clamp(demage, 10, 9999);

            //更新属性
            switch (target._entityType)
            {
                case EntityType.Player:
                    (target as Player)?.GetHurt(demage);
                    break;
                case EntityType.Monster:
                    (target as Monster)?.GetHurt(attacker, demage);
                    break;
            }

            return demage;
        }

        //获取技能伤害
        public int GetSkillHurt(Player attacker, Entity target)
        {
            //基础伤害
            int demage = DataManager.Instance.GetUnitDefine
                (attacker._unitDefine.SkillUnitId).AttackDemage[0];

            //等级加成
            demage += (int)(attacker._dbCharacter.Level * 10f);

            //目标防御
            if(target is Player player)
            {
                var attribute = AttributeManager.Instance.GetPlayerAttribute(player._user._userId);
                if(attribute != null)
                {
                    demage -= attribute._defAddition;
                }
            }

            //更新属性
            switch (target._entityType)
            {
                case EntityType.Player:
                    (target as Player)?.GetHurt(demage);
                    break;
                case EntityType.Monster:
                    (target as Monster)?.GetHurt(attacker, demage);
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
                //增加任务进度--若有
                if(deadMonster._unitDefine.ID == 6 || deadMonster._unitDefine.ID == 7)
                {
                    TaskManager.Instance.UpdateTask(player._user._userId, 2, 1);
                }
            }

            (deadMonster as Monster)?._controller.ChangeState(MonsterState.die);
        }
    }
}
