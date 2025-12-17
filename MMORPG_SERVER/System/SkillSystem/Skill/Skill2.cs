using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MissileSystem;
using MMORPG_SERVER.System.UserSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.SkillSystem.Skill
{
    //角色2技能--追踪导弹
    public class Skill2 : ISkill
    {
        public async Task UseSkill(User user)
        {
            //生成导弹
            var target = EntityManager.Instance.GetClosedEntity(user._player);
            if (target == null) return;
            Log.Information($"[SkillManager]生成导弹，追踪{target._entityId}");

            var missile = new MissileAi
                (EntityManager.Instance.NewEntityId(),
                EntityType.Missile,
                DataManager.Instance.GetUnitDefine(user._player._unitDefine.SkillUnitId),
                user._player._position + new Vector3(0, 2, 0),
                0,
                target,
                null,
                8f,
                user._player,
                1f);

            MissileManager.Instance.AddMissile(missile);
        }
    }
}
