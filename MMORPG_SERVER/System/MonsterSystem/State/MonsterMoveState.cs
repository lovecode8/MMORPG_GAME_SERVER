using MMORPG_SERVER.System.EntitySystem;
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

        private float _moveSpeed = 5f;

        private Vector3 _currentTarget;

        public MonsterMoveState(MonsterAi monsterAi, List<Vector3> list)
        {
            _monsterAi = monsterAi;
            _movePosition = list;
        }

        public bool Condition(IState newState)
        {
            return true;
        }

        public void Enter()
        {
            Log.Information("move");
            _monsterAi._monster._stateId = (int)MonsterState.move;
            _currentTarget = _movePosition[_currentMoveIndex];
        }

        public void Exit()
        {
            
        }

        public void FixedUpdate()
        {
            
        }

        public void Update()
        {
            UpdatePostion();
            UpdateRotation();

            if(Vector3.Distance(_monsterAi._monster._position, _currentTarget) < 0.5f)
            {
                _monsterAi.ChangeState(MonsterState.idle);
                _currentMoveIndex = (_currentMoveIndex + 1) % _movePosition.Count;
                _currentTarget = _movePosition[_currentMoveIndex];
            }
        }

        private bool MoveCondition()
        {
            return Vector3.Distance(_monsterAi._monster._position, _currentTarget) > 0.1f;
        }

        private void UpdatePostion()
        {
            if (MoveCondition())
            {
                Vector3 direction = _currentTarget - _monsterAi._monster._position;
                direction.Y = 0;
                direction = Vector3.Normalize(direction);

                Vector3 moveDelta = _moveSpeed * direction * MMORPG_SERVER.Time.Timer.deltaTime;
                _monsterAi._monster._position += moveDelta;
                Log.Information(_monsterAi._monster._position.ToString());
            }
        }

        private void UpdateRotation()
        {
            // 1. 计算方向向量（排除Y轴）
            Vector3 direction = _currentTarget - _monsterAi._monster._position;
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
