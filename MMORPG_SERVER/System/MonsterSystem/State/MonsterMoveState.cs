using Extension;
using MMORPG_SERVER.System.AStarSystem;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.Tool;
using Org.BouncyCastle.Asn1.X509;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.MonsterSystem.State
{
    public class MonsterMoveState : IState
    {
        private MonsterAi _monsterAi;

        private List<Vector3> _movePosition;

        private int _currentMoveIndex = 0;

        private float _moveSpeed = 3.5f;

        private Vector3 _currentTarget;

        private List<Vector3> _currentPath = new();

        private int _currentPathIndex = 0;

        private Vector3 _currentPathTarget;

        public MonsterMoveState(MonsterAi monsterAi, List<Vector3> list)
        {
            _monsterAi = monsterAi;
            _movePosition = list;
        }

        public bool Condition(IState newState)
        {
            return true;
        }

        public void Enter() => _ = EnterAsync();

        private async Task EnterAsync()
        {
            Log.Information("move");
            _monsterAi._monster._stateId = (int)MonsterState.move;
            _currentTarget = _movePosition[_currentMoveIndex];
            _currentTarget.Y = _monsterAi._monster._position.Y;

            _currentPath.Clear();

            var startPos = new Vector3(_monsterAi._monster._position.X, -2, _monsterAi._monster._position.Z);

            var newPath = await AStarManager.Instance.GetAStarPath(startPos, _currentTarget);

            if(newPath == null)
            {
                Log.Information("路径获取失败");
                _monsterAi._monster._position += new Vector3(0.1f, 0, 0.1f);
                _monsterAi.ChangeState(MonsterState.idle);
                return;
            }

            _currentPath = newPath;
            _currentPathIndex = 0;
            _currentPathTarget = _currentPath[_currentPathIndex];
            _currentPathTarget.Y = _monsterAi._monster._position.Y;
        }

        public void Exit()
        {
            
        }

        public void FixedUpdate()
        {
            
        }

        public void Update()
        {
            //判断追逐
            var player = PlayerManager.Instance.GetChaseablePlayer(_monsterAi._monster);
            if (player != null)
            {
                _monsterAi.SetChaseTarget(player);
                _monsterAi.ChangeState(MonsterState.chase);
            }

            //到达目的地
            if (Vector3.Distance(_monsterAi._monster._position, _currentTarget) < 0.5f)
            {
                _currentMoveIndex = (_currentMoveIndex + 1) % _movePosition.Count;
                _monsterAi.ChangeState(MonsterState.idle);
                return;
            }

            UpdatePostion();
            UpdateRotation();
            Log.Information(Vector3.Distance(_monsterAi._monster._position, _currentTarget).ToString());
        }

        private void UpdatePostion()
        {
            Vector3 direction = _currentPathTarget - _monsterAi._monster._position;
            direction.Y = 0;

            // 使用平滑插值而不是直接归一化
            float distance = direction.Length();
            if (distance > 0.1f)
            {
                direction = Vector3.Normalize(direction);
                direction.Y = 0;
                Vector3 moveDelta = _moveSpeed * direction * MMORPG_SERVER.Time.Timer.deltaTime;

                // 防止 overshoot
                if (moveDelta.Length() > distance)
                {
                    _monsterAi._monster._position = _currentPathTarget;
                }
                else
                {
                    _monsterAi._monster._position += moveDelta;
                }
            }
            if(distance < 0.3f && _currentPathIndex < _currentPath.Count - 1)
            {
                _monsterAi._monster._position = _currentPathTarget;
                _currentPathTarget = _currentPath[++_currentPathIndex];
                _currentPathTarget.Y = _monsterAi._monster._position.Y;
            }
        }

        private void UpdateRotation()
        {
            // 1. 计算方向向量（排除Y轴）
            Vector3 direction = _currentPathTarget - _monsterAi._monster._position;
            direction.Y = 0;

            // 2. 计算Yaw角（绕Y轴旋转，弧度转角度）
            // 公式：Yaw = atan2(direction.X, direction.Z) × (180/π)
            float yawRadians = (float)Math.Atan2(direction.X, direction.Z);
            float yawDeg = yawRadians * (180 / (float)Math.PI);

            // 3. 更新怪物的旋转（存储Yaw角即可，客户端渲染时用）
            _monsterAi._monster._rotationY = yawDeg;
        }
    }
}
