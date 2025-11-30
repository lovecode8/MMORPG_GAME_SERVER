using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.AttributeSystem;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.FightSystem;
using MMORPG_SERVER.System.MonsterSystem;
using MMORPG_SERVER.System.PlayerSystem;
using Serilog;

namespace MMORPG_SERVER.Service
{
    public class FightService : ServiceBase<FightService>
    {

        //处理玩家攻击请求
        public void OnHandle(object sender, PlayerAttackRequest playerAttackRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                int playerId = playerAttackRequest.PlayerId;
                int targetId = playerAttackRequest.TargetId;
                var attacker = EntityManager.Instance.GetEntity(playerId);
                var target = EntityManager.Instance.GetEntity(targetId);

                Log.Information($"[FightService] 收到攻击请求：{playerId}攻击{targetId}");

                //未命中
                if (playerAttackRequest.TargetId == -1 || 
                !EntityManager.Instance.IsAttackTargetVaild(attacker, target, 5f))
                {
                    //仅同步动画
                    PlayerManager.Instance.Broadcast(new PlayerAttackResponse()
                    {
                        IsSuccessfulAttack = false,
                        PlayerId = playerId,
                        CamboCount = playerAttackRequest.CamboCount,
                        IsHit = false
                    }, channel._user._player);
                    return;
                }

                //命中
                if(target is Monster monster)
                {
                    monster._controller.ChangeState(MonsterState.getHit);
                }

                var demage = FightManager.Instance.
                    GetFightHurtValue(attacker, target, playerAttackRequest.CamboCount);

                Log.Information($"[FightService] 攻击结果：对{targetId}造成{demage}点伤害");

                //发给所有客户端
                PlayerManager.Instance.Broadcast(new PlayerAttackResponse()
                {
                    IsSuccessfulAttack = true,
                    PlayerId = playerId,
                    TargetId = targetId,
                    CamboCount = playerAttackRequest.CamboCount,
                    IsHit = true,
                    Damage = (int)demage
                }, channel._user._player, true);
            });
        }
    }
}
