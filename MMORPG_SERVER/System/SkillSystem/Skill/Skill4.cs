using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.FightSystem;
using MMORPG_SERVER.System.MissileSystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.System.UserSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.SkillSystem.Skill
{
    //角色4技能--山崩地裂
    public class Skill4 : ISkill
    {
        public async Task UseSkill(User user)
        {
            var instantitePos = user._player._position +
                Vector3Extensions.CalculateForwardDirection(user._player._rotationY);
            var missile = new MissileAi
                (EntityManager.Instance.NewEntityId(),
                EntityType.Missile,
                DataManager.Instance.GetUnitDefine(user._player._unitDefine.SkillUnitId),
                instantitePos,
                user._player._rotationY - 90,
                null,
                null,
                0,
                user._player,
                0);
            await Task.Delay(700);
            MissileManager.Instance.AddMissile(missile);

            //获取沿途的实体造成伤害
            var entityList = EntityManager.Instance.GetWayEntity(user._player);
            if (entityList.Count > 0)
            {
                foreach (var entity in entityList)
                {
                    PlayerManager.Instance.Broadcast(new PlayerAttackResponse()
                    {
                        IsSuccessfulAttack = true,
                        IsHit = true,
                        Damage = FightManager.Instance.GetSkillHurt(user._player, entity),
                        PlayerId = -1,
                        TargetId = entity._entityId
                    }, user._player, true);
                }
            }

            await Task.Delay(2000);
            //删除实体
            MissileManager.Instance.RemoveMissile(missile);
        }
    }
}
