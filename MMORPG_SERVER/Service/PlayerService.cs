using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.PlayerSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Service
{
    public class PlayerService : ServiceBase<PlayerService>
    {
        public void OnConectionClosed(NetChannel sender)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                Log.Information($"[PlayerService] 玩家离开游戏：{sender._user?._player?._playerId}");
                if(sender._user?._player != null)
                {
                    PlayerManager.Instance.RemovePlayer(sender._user._player);
                }
                MapManager.Instance.EntityLeave(sender._user._player);
                sender._user = null;
            });
        }

        public void OnHandle(object sender, JoinMapRequest joinMapRequest)
        {
            UpdateManager.Instance.AddTask(async() =>
            {
                NetChannel? channel = sender as NetChannel;
                Log.Information($"[PlayerService]请求加入游戏：{channel?._user._userId}");

                if(channel?._user == null)
                {
                    Log.Information($"[PlayerService] 加入失败：用户未登录");
                    return;
                }

                if(channel._user._player != null)
                {
                    Log.Information("[PlayerService] 加入失败：玩家已进入游戏");
                    return;
                }

                var dbCharacter = await MysqlManager.Instance._freeSql.Select<DbCharacter>().
                                    Where(c => c.UserId == channel._user._userId).
                                    FirstAsync();
                if(dbCharacter == null)
                {
                    Log.Information("[PlayerService] 加入失败：玩家角色信息不存在");
                    return;
                }
                Vector3 pos = new Vector3(dbCharacter.posX, dbCharacter.posY, dbCharacter.posZ);
                Vector3 rot = new Vector3(dbCharacter.rotX, dbCharacter.rotY, dbCharacter.rotZ);
                int playerId = EntityManager.Instance.NewEntityId();
                
                //发送加入游戏回复
                channel.SendAsync(new JoinMapResponse() 
                { JoinMapResult = JoinMapResult.Success, EntityId = playerId, NetCharacter = dbCharacter.ToNetCharacter() });
                
                var player = PlayerManager.Instance.NewPlayer(playerId, channel._user, dbCharacter, pos, rot);
                channel._user.SetPlayer(player);
                Log.Information($"[PlayerService] 加入成功：{channel._user._userId}成功加入游戏");
            });
        }
    }
}
