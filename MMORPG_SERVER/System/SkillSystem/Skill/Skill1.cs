using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.FightSystem;
using MMORPG_SERVER.System.MissileSystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.System.UserSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.SkillSystem.Skill
{
    //角色1技能--裂地劈（对自己范围内10米的敌人造成伤害）
    public class Skill1 : ISkill
    {
        private float _interactDistance = 10f;

        public async Task UseSkill(User user)
        {
            var missile = new MissileAi
                (EntityManager.Instance.NewEntityId(),
                EntityType.Missile,
                DataManager.Instance.GetUnitDefine(user._player._unitDefine.SkillUnitId),
                user._player._position,
                0,
                null,
                null,
                0,
                user._player,
                _interactDistance);

            await Task.Delay(600);
            MissileManager.Instance.AddMissile(missile);

            //获取范围内的实体
            var entityList = EntityManager.Instance.GetEntityListWithRange(user._player._position, _interactDistance);
            if(entityList.Count > 0)
            {
                foreach(var entity in entityList)
                {
                    if (entity == user._player) continue;
                    int demage = FightManager.Instance.GetSkillHurt(user._player, entity);
                    PlayerManager.Instance.Broadcast(new PlayerAttackResponse()
                    {
                        IsSuccessfulAttack = true,
                        IsHit = true,
                        PlayerId = -1,
                        TargetId = entity._entityId,
                        Damage = demage
                    }, entity, true);
                }
            }
        }
    }
}
