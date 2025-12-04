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

        //各个玩家技能冷却时间
        private Dictionary<int, float> _playerSkillColdTimeDict = new();

        private List<int> _tempUserIdList = new();

        private Random _random = new Random();

        public void Update()
        {
            _tempUserIdList.Clear();

            foreach(int userId in _playerSkillColdTimeDict.Keys)
            {
                _tempUserIdList.Add(userId);
            }

            foreach(var key in _tempUserIdList)
            {
                if(_playerSkillColdTimeDict.TryGetValue(key, out var time))
                {
                    time -= MMORPG_SERVER.Time.Timer.deltaTime;
                    _playerSkillColdTimeDict[key] = time;

                    if(time < 0)
                    {
                        _playerSkillColdTimeDict.Remove(key);
                    }
                }
            }
        }

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
            demage = (int)(demage * _random.NextSingle() * 1.5);

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

            (deadMonster as Monster)._controller.ChangeState(MonsterState.die);
        }
    }
}
