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
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Numerics;
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

        //同步单个实体信息给视野范围内的玩家
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
            Vector2 entityCell = GetCellByPosition(entity._position);

            for(int x = (int)entityCell.X - 1; x <= entityCell.X + 1; x++)
            {
                for(int y = (int)entityCell.Y - 1; y <= entityCell.Y + 1; y++)
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
                                //Log.Information($"超过距离{distance}");
                                continue;
                            }

                            //超过指定角度(超过90度且距离大于15)
                            if (! IsIn180DegreeView(player, entity) && distance > 15f)
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

        /// <summary>
        /// 判断被观察者是否在观察者的180度前方视野内
        /// </summary>
        /// <param name="player">观察者</param>
        /// <param name="entity">被观察者</param>
        /// <returns>是否在视野内</returns>
        public bool IsIn180DegreeView(Player player, Entity entity)
        {
            // 1. 计算方向向量（被观察者 - 观察者）
            Vector3 dir = new Vector3(
                entity._position.X - player._position.X,
                entity._position.Y - player._position.Y,
                entity._position.Z - player._position.Z
            );

            // 2. 处理为水平方向向量（忽略Y轴）并归一化
            Vector3 horizontalDir = new Vector3(dir.X, 0, dir.Z);
            float dirLength = (float)Math.Sqrt(horizontalDir.X * horizontalDir.X + horizontalDir.Z * horizontalDir.Z);
            // 若两点重合（方向向量长度为0），视为在视野内
            if (dirLength < 1e-10f)
                return true;
            // 归一化（单位向量）
            horizontalDir = new Vector3(
                horizontalDir.X / dirLength,
                0,
                horizontalDir.Z / dirLength
            );

            // 3. 计算观察者的正前方水平向量（基于Y轴旋转角）
            // 角度转弧度（System.Math的三角函数使用弧度）
            double radians = player._rotationY * Math.PI / 180.0;
            // 正前方向量（XZ平面）：X = sin(θ), Z = cos(θ)
            Vector3 forward = new Vector3(
                (float)Math.Sin(radians),  // X分量
                0,
                (float)Math.Cos(radians)   // Z分量
            );
            // 归一化（理论上已单位化，保险起见再归一化）
            float forwardLength = (float)Math.Sqrt(forward.X * forward.X + forward.Z * forward.Z);
            if (forwardLength > 1e-10f)
            {
                forward = new Vector3(
                    forward.X / forwardLength,
                    0,
                    forward.Z / forwardLength
                );
            }

            // 4. 计算点积：点积 ≥ 0 → 夹角 ≤ 90度（在180度视野内）
            float dotProduct = horizontalDir.X * forward.X + horizontalDir.Z * forward.Z;
            return dotProduct >= 0;
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
