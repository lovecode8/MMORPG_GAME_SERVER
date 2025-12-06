using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MissileSystem;
using MMORPG_SERVER.System.UserSystem;
using Serilog;
using System.Numerics;

namespace MMORPG_SERVER.System.SkillSystem.Skill
{
    //角色3技能--火球
    //向前方发射一个火球，最远向前飞10米，如果飞行过程中碰到实体会提前爆炸。
    public class Skill3 : ISkill
    {
        //最远距离
        private float _maxDistance = 10f;

        public async Task UseSkill(User user)
        {
            var missileDestination = user._player._position +
                Vector3Extensions.CalculateForwardDirection(user._player._rotationY) * 15;
            Log.Information($"{user._player._position}   {missileDestination}");

            var missile = new MissileAi(EntityManager.Instance.NewEntityId(),
                EntityType.Missile,
                DataManager.Instance.GetUnitDefine(user._player._unitDefine.SkillUnitId),
                user._player._position + new Vector3(0, 2, 0),
                0,
                null,
                missileDestination,
                5f,
                user._player,
                3f);

            await Task.Delay(500);
            MissileManager.Instance.AddMissile(missile);
        }
    }
}
