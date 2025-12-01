using Extension;
using Google.Protobuf;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.MonsterSystem;
using MMORPG_SERVER.System.UserSystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
namespace MMORPG_SERVER.System.PlayerSystem
{
    //玩家管理器
    public class PlayerManager : Singleton<PlayerManager>
    {
        private Dictionary<int, Player> _playerDictionary = new();

        private Dictionary<int, Player> _leavePlayerQueue = new();

        //网格对应玩家列表
        private ConcurrentDictionary<Vector2, HashSet<Player>> _cellPlayers = new();

        private float _playerVisibleDistance = 50f;

        private float _playerVisibleAngle = 90f;

        private PlayerManager() { }

        public Player NewPlayer(int playerId, User user, DbCharacter dbCharacter, Vector3 pos, float dir)
        {
            var player = new Player
                (playerId,
                EntityType.Player,
                DataManager.Instance.GetUnitDefine(dbCharacter.UnitId),
                pos,
                dir,
                user,
                dbCharacter
                );
            AddPlayer(player);

            EntityManager.Instance.AddEntity(player);
            MapManager.Instance.EntityEnter(player);

            var cell = GetCellByPosition(pos);
            _cellPlayers.GetOrAdd(cell, _ => new HashSet<Player>());
            _cellPlayers[cell].Add(player);
            return player;
        }

        //获取在线玩家数量
        public int GetPlayerCount()
        {
            return _playerDictionary.Count;
        }

        //玩家移动--更新网格信息
        public void OnPlayerMove(Player player)
        {
            var newCell = GetCellByPosition(player._position);
            if(newCell == player._currentCell)
            {
                return;
            }

            if(_cellPlayers.TryGetValue(player._currentCell, out var oldSet))
            {
                lock (oldSet)
                {
                    oldSet.Remove(player);
                }
            }

            _cellPlayers.GetOrAdd(newCell, _ => new HashSet<Player>());
            lock (_cellPlayers[newCell])
            {
                _cellPlayers[newCell].Add(player);
            }

            player._currentCell = newCell;
        }

        public Vector2 GetCellByPosition(Vector3 pos)
        {
            return new Vector2()
            {
                X = (int)Math.Floor(pos.X / 50),
                Y = (int)Math.Floor(pos.Z / 50)
            };
        }

        //同步所有实体信息给有视野玩家
        public void SyncAllEntityData(Dictionary<int, Entity> entityDatas)
        {
            lock (_playerDictionary)
            {
                if (_playerDictionary.Count == 0) return;

                foreach (var entity in entityDatas.Values)
                {
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

        //获取视野范围内的玩家
        private List<Player> GetVisiblePlayerList(Entity entity)
        {
            List<Player> res = new();
            Vector2 entityCell = GetCellByPosition(entity._position);

            for(int x = (int)entityCell.X - 1; x <= (int)entityCell.X + 1; x++)
            {
                for(int y = (int)entityCell.Y - 1; y <= (int)entityCell.Y + 1; y++)
                {
                    if(_cellPlayers.TryGetValue(new Vector2(x, y), out var list))
                    {
                        foreach(var player in list)
                        {
                            //是自己不操作
                            if (entity is Player && (entity as Player) == player)
                            {
                                continue;
                            }
                            //超过指定距离
                            float distance = Vector3.Distance(entity._position, player._position);
                            if (distance > _playerVisibleDistance)
                            {
                                Log.Information($"超过距离{distance}");
                                continue;
                            }

                            //超过指定角度(超过90度且距离大于15)
                            Vector3 dir = entity._position - player._position;
                            if (! Vector3Extensions.IsInAngleRange(dir, player._rotationY, 180) && distance > 15f)
                            {
                                Log.Information($"超过角度 {distance}");
                                continue;
                            }

                            res.Add(player);
                        }
                    }
                    
                }
            }
            return res;
        }

        public Player? GetChaseablePlayer(Monster monster)
        {
            Vector2 cell = GetCellByPosition(monster._position);
            for(int x = (int)cell.X - 1; x < (int)cell.X + 1; x++)
            {
                for(int y = (int)cell.Y - 1; y < (int)cell.Y + 1; y++)
                {
                    if(_cellPlayers.TryGetValue(new Vector2(x, y), out var list))
                    {
                        foreach(var player in list)
                        {
                            var distance = Vector3.Distance(monster._position, player._position);
                            if(distance < 10f)
                            {
                                return player;
                            }
                        }
                    }
                }
            }
            return null;
        }

        //广播消息
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

            var cell = GetCellByPosition(player._position);
            if(_cellPlayers.TryGetValue(cell, out var list))
            {
                lock (list)
                {
                    list.Remove(player);
                }
            }
        }
    }
}
