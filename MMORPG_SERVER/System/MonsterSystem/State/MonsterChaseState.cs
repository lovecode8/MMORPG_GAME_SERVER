using MMORPG_SERVER.Extension;
using MMORPG_SERVER.System.AStarSystem;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.MonsterSystem.State
{
    public class MonsterChaseState : IState
    {
        private MonsterAi _monsterAi;

        private float _chaseSpeed = 5f;

        private float _chaseMaxDistance = 20f;

        private float _attackDistance = 3f;

        private List<Vector3> _chasePathList = new();

        private int _currentPathIndex = 0;

        private Vector3 _currentPathTarget;

        private Vector3 _targetLastPosition;

        private float _updatePathInterval = 1f;

        private float _timer;

        private bool _isUpdatingPath;

        public MonsterChaseState(MonsterAi monsterAi)
        {
            _monsterAi = monsterAi;
        }

        public bool Condition(IState newState)
        {
            return true;
        }

        public void Enter() => _ = EnterAsync();

        private async Task EnterAsync()
        {
            Log.Information("Chase");
            _monsterAi._monster._stateId = (int)MonsterState.chase;

            await UpdatePath();

            _targetLastPosition = _monsterAi._chaseTarget._position;
            _timer = 0;
        }

        public void Exit()
        {
            _chasePathList.Clear();
            _currentPathIndex = 0;
            _timer = 0;
            _isUpdatingPath = false;
        }

        public void FixedUpdate()
        {
            
        }

        public void Update()
        {
            CheckUpdatePath();
            ChaseTarget();
        }

        private void ChaseTarget()
        {
            var distanceToTarget =
                Vector3.Distance(_monsterAi._monster._position, _monsterAi._chaseTarget._position);

            //攻击
            if (distanceToTarget < _attackDistance && _monsterAi._canAttack)
            {
                _monsterAi.ChangeState(MonsterState.attack);
                _ = _monsterAi.LockAttack();
            }
            //返回
            else if (distanceToTarget > _chaseMaxDistance)
            {
                _monsterAi.ChangeState(MonsterState.move);
            }

            var distanceToNextPoint = Vector3.DistanceSquared
                (_monsterAi._monster._position, _currentPathTarget);

            //切换寻路点
            if (distanceToNextPoint < 9f)
            {
                if(_currentPathIndex < _chasePathList.Count - 1)
                {
                    _currentPathIndex++;
                    _currentPathTarget = _chasePathList[_currentPathIndex];
                }
                else
                {
                    _ = UpdatePath();
                }
            }
            //追逐
            else
            {
                //pos
                var direction = _currentPathTarget - _monsterAi._monster._position;
                direction.Y = 0;
                var moveDelta =
                    Vector3.Normalize(direction) * _chaseSpeed * MMORPG_SERVER.Time.Timer.deltaTime;
                _monsterAi._monster._position += moveDelta;
                Log.Information(_monsterAi._monster._position.ToString());

                //rot
                float yawRadians = (float)Math.Atan2(direction.X, direction.Z);
                float yawDeg = yawRadians * (180 / (float)Math.PI);
                _monsterAi._monster._rotationY = yawDeg;
            }
        }

        //检查是否需要更新路径
        private void CheckUpdatePath()
        {
            _timer += MMORPG_SERVER.Time.Timer.deltaTime;
            var distance = 
                Vector3.DistanceSquared(_monsterAi._chaseTarget._position, _targetLastPosition);
            
            //目标偏移
            if (distance > 1f)
            {
                _ = UpdatePath();
                _timer = 0;
                _targetLastPosition = _monsterAi._chaseTarget._position;
            }
            
            //达到指定时间
            if(_timer > _updatePathInterval && distance > 0.1f)
            {
                _ = UpdatePath();
                _timer = 0;
                _targetLastPosition = _monsterAi._chaseTarget._position;
            }
        }

        //更新路径
        private async Task UpdatePath()
        {
            if (_isUpdatingPath || _monsterAi._chaseTarget == null) return;
            _isUpdatingPath = true;

            try
            {
                var startPos = _monsterAi._monster._position;
                startPos.Y = -2;

                var endPos = _monsterAi._chaseTarget._position;
                endPos.Y = -2;

                var list = await AStarManager.Instance.
                    GetAStarPath(startPos, endPos);
                if (list == null)
                {
                    Log.Information("目标点不可达，追逐结束");
                    _monsterAi.ChangeState(MonsterState.move);
                    return;
                }

                _currentPathIndex = 0;
                _chasePathList = list;
                _currentPathTarget = _chasePathList[_currentPathIndex];
            }
            catch (Exception ex)
            {
                Log.Information(ex.Message);
            }
            finally
            {
                _isUpdatingPath = false;
            }
        }
    }
}
