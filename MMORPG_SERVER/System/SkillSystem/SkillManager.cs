using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.FightSystem;
using MMORPG_SERVER.System.MapSystem;
using MMORPG_SERVER.System.MissileSystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.System.SkillSystem.Skill;
using MMORPG_SERVER.System.UserSystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.SkillSystem
{
    public interface ISkill
    {
        Task UseSkill(User user);
    }

    public class SkillManager : Singleton<SkillManager>
    {
        private SkillManager() { }

        //角色Id对应技能
        private Dictionary<int, ISkill> _characterSkillDict = new();

        //各个玩家技能冷却时间
        private Dictionary<int, int> _playerSkillColdTimeDict = new();

        private List<int> _tempUserIdList = new();

        private float _updateSkillTimeInterval = 1f;

        private float _timer;

        public void Start()
        {
            LoadSkill();
        }

        public void Update()
        {
            UpdatePlayerSkillColdTime();
        }

        //加载技能
        private void LoadSkill()
        {
            _characterSkillDict.Add(1, new Skill1());
            _characterSkillDict.Add(2, new Skill2());
            _characterSkillDict.Add(3, new Skill3());
            _characterSkillDict.Add(4, new Skill4());
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
                _ = Task.Run(() =>
                {
                    _characterSkillDict[user._player._dbCharacter.UnitId].UseSkill(user);
                });
                return true;
            }
        }
    }
}
