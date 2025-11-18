using Extension;
using Google.Protobuf;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.UserSystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System.Net;
using System.Numerics;
namespace MMORPG_SERVER.System.PlayerSystem
{
    //玩家管理器
    public class PlayerManager : Singleton<PlayerManager>
    {
        private Dictionary<int, Player> _playerDictionary = new();

        private Dictionary<int, Player> _leavePlayerQueue = new();

        private float _playerVisibleDistance = 50f;

        private float _playerVisibleAngle = 360f;

        private PlayerManager() { }

        public Player NewPlayer(int playerId, User user, DbCharacter dbCharacter, Vector3 pos, float dir)
        {
            var player = new Player
                (playerId,
                EntityType.Player,
                DataManager.Instance.GetCharacterDefine(dbCharacter.UnitId),
                pos,
                dir,
                user,
                dbCharacter.Gold
                );

            EntityManager.Instance.AddEntity(player);
            AddPlayer(player);
            MapManager.Instance.EntityEnter(player);
            return player;
        }

        //同步所有实体信息给有视野玩家
        public void SyncAllEntityData(Dictionary<int, Entity> entityDatas)
        {
            lock (_playerDictionary)
            {
                if (_playerDictionary.Count == 0) return;

                foreach (var entity in entityDatas.Values)
                {
                    //var playerList = _playerDictionary.Values;
                    var playerList = GetVisiblePlayerList(entity);
                    if (playerList.Count == 0) continue;

                    foreach (var player in playerList)
                    {
                        player._user._netChannel.SendAsync(new EntitySyncResponse()
                        {
                            EntityId = entity._entityId,
                            Position = entity._position.ToNetVector3(),
                            RotationY = entity._rotationY,
                            StateId = entity._stateId
                        });
                    }
                }
            }
            
        }

        //同步实体信息给视野范围内的玩家
        public void SyncSingleEntityData(Entity entity)
        {
            var playerList = GetVisiblePlayerList(entity);
            if(playerList.Count > 0)
            {
                foreach(var player in playerList)
                {
                    player._user._netChannel.SendAsync(new EntitySyncResponse()
                    {
                        EntityId = entity._entityId,
                        Position = entity._position.ToNetVector3(),
                        RotationY = entity._rotationY,
                        StateId = entity._stateId
                    });
                }
            }
        }

        //获取视野范围内的玩家
        private List<Player> GetVisiblePlayerList(Entity entity)
        {
            List<Player> res = new();

            lock (_playerDictionary)
            {
                foreach (var player in _playerDictionary.Values)
                {
                    //是自己不操作
                    if (entity is Player && (entity as Player) == player) continue;

                    //超过指定距离
                    float distance = Vector3.Distance(entity._position, player._position);
                    if (distance > _playerVisibleDistance) continue;

                    //超过指定角度
                    Vector3 dir = entity._position - player._position;
                    float angle = dir.Angle(player._rotationY);
                    if (angle > _playerVisibleAngle / 2) continue;

                    res.Add(player);
                }
            }
            return res;
        }

        public void Broadcast(IMessage message, Entity sender, bool sendToSelf = false)
        {
            foreach(Player player in _playerDictionary.Values)
            {
                if(!sendToSelf && sender is Player && (sender as Player) == player)
                {
                    continue;
                }
                player._user._netChannel.SendAsync(message);
            }
        }

        public void AddPlayer(Player player)
        {
            lock (_playerDictionary)
            {
                if (!_playerDictionary.ContainsKey(player._playerId))
                {
                    _playerDictionary.Add(player._playerId, player);
                }
            }
        }

        public void RemovePlayer(Player player)
        {
            lock (_playerDictionary)
            {
                if (_playerDictionary.ContainsKey(player._playerId))
                {
                    EntityManager.Instance.RemoveEntity(player);
                    _playerDictionary.Remove(player._playerId);
                }
            }
        }
    }
}
