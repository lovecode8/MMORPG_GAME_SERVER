using Google.Protobuf;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.UserSystem;
using MMORPG_SERVER.Tool;
using System.Numerics;
namespace MMORPG_SERVER.System.PlayerSystem
{
    //玩家管理器
    public class PlayerManager : Singleton<PlayerManager>
    {
        private Dictionary<int, Player> _playerDictionary = new();

        private Dictionary<int, Player> _leavePlayerQueue = new();

        private PlayerManager() { }

        public Player NewPlayer(int playerId, User user, DbCharacter dbCharacter, Vector3 pos, Vector3 dir)
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
            _playerDictionary.Add(player._playerId, player);
            MapManager.Instance.EntityEnter(player);
            return player;
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

        public void RemovePlayer(Player player)
        {
            if (_playerDictionary.ContainsKey(player._playerId))
            {
                EntityManager.Instance.RemoveEntity(player);
                _playerDictionary.Remove(player._playerId);
            }
        }
    }
}
