using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.EntitySystem;
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
                Log.Information($"[FightService] 收到攻击请求：{playerId}攻击{targetId}");

                if (playerAttackRequest.TargetId == -1)
                {
                    //仅同步动画
                    PlayerManager.Instance.Broadcast(new PlayerAttackResponse()
                    {
                        IsSuccessfulAttack = true,
                        PlayerId = playerId,
                        CamboCount = playerAttackRequest.CamboCount,
                        IsHit = false
                    }, channel._user._player);
                }

                //var entity = EntityManager.Instance.GetEntity(targetId);
                //if(entity._entityType == EntityType.Player)
                //{

                //}
            });
        }
    }
}
