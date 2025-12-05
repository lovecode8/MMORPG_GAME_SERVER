using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.MissileSystem;
using MMORPG_SERVER.System.UserSystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.SkillSystem
{
    public class SkillManager : Singleton<SkillManager>
    {
        private SkillManager() { }

        //各个玩家技能冷却时间
        private Dictionary<int, int> _playerSkillColdTimeDict = new();

        private List<int> _tempUserIdList = new();

        private Random _random = new Random();

        private float _updateSkillTimeInterval = 1f;

        private float _timer;

        public void Update()
        {
            UpdatePlayerSkillColdTime();
        }

        //更新玩家的技能冷却时间
        private void UpdatePlayerSkillColdTime()
        {
            _timer += MMORPG_SERVER.Time.Timer.deltaTime;

            if(_timer > _updateSkillTimeInterval)
            {
                _tempUserIdList.Clear();

                lock(_playerSkillColdTimeDict)
                {
                    foreach (int userId in _playerSkillColdTimeDict.Keys)
                    {
                        _tempUserIdList.Add(userId);
                    }

                    foreach (var key in _tempUserIdList)
                    {
                        if (_playerSkillColdTimeDict.TryGetValue(key, out var time))
                        {
                            time--;
                            _playerSkillColdTimeDict[key] = time;

                            if (time < 0)
                            {
                                _playerSkillColdTimeDict.Remove(key);
                            }
                        }
                    }
                }

                _timer = 0;
            }
        }

        //增加技能冷却
        public void AddNewSkillColdTime(User user)
        {
            lock(_playerSkillColdTimeDict)
            {
                if (!_playerSkillColdTimeDict.ContainsKey(user._userId))
                {
                    var coldTime = user._player._unitDefine.SkillInterval;
                    _playerSkillColdTimeDict[user._userId] = coldTime;
                }
            }
        }

        //使用技能
        public bool UseSkill(User user)
        {
            lock(_playerSkillColdTimeDict)
            {
                if (_playerSkillColdTimeDict.ContainsKey(user._userId))
                {
                    return false;
                }

                //更新冷却
                _playerSkillColdTimeDict[user._userId] = user._player._unitDefine.SkillInterval;

                //释放技能
                ShowSkill(user);
                return true;
            }
        }

        private void ShowSkill(User user)
        {
            switch(user._player._unitDefine.ID)
            {
                case 1:
                    Player1Skill();
                    break;
                case 2:
                    Player2Skill(user);
                    break;
            }
        }

        private void Player1Skill()
        {

        }

        private void Player2Skill(User user)
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
                5f,
                user._player,
                2f);

            MissileManager.Instance.AddMissile(missile);
        }
    }
}
