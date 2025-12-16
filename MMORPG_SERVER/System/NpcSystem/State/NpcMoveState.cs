using MMORPG_SERVER.System.AStarSystem;
using MMORPG_SERVER.System.MonsterSystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Mysqlx.Crud.Order.Types;

namespace MMORPG_SERVER.System.NpcSystem.State
{
    public class NpcMoveState : IState
    {
        private NpcAi _npcAi;

        private List<Vector3> _posList = new();

        private int _currentIndex = 0;

        private Vector3 _currentTarget;

        private List<Vector3> _currentPathList = new();

        private int _currentPathIndex;

        private Vector3 _currentPathTarget;

        private float _moveSpeed = 3f;

        public NpcMoveState(NpcAi npcAi, List<Vector3> movePos)
        {
            _npcAi = npcAi;
            _posList = movePos;
            _currentIndex = 0;
            _currentPathIndex = 0;
        }

        public bool Condition(IState newState)
        {
            return true;
        }

        public void Enter()
        {
            _ = Task.Run(async () =>
            {
                await EnterAsync();
            });
        }

        private async Task EnterAsync()
        {
            Log.Information("npcMove");
            _npcAi._npc._stateId = (int)NpcState.move;
            _currentPathIndex = 0;
            _currentIndex = (_currentIndex + 1) % _posList.Count;
            _currentTarget = _posList[_currentIndex];
            _currentPathList = await AStarManager.Instance.GetAStarPath(_npcAi._npc._position, _currentTarget);
            _currentPathTarget = _currentPathList[_currentPathIndex];
        }

        public void Exit()
        {
            
        }

        public void FixedUpdate()
        {
            
        }

        public void Update()
        {
            MoveToTarget();
        }

        private void MoveToTarget()
        {
            if (_currentPathList.Count == 0) return;

            //到达目标点
            var distanceToTarget = Vector3.DistanceSquared(_npcAi._npc._position, _currentTarget);
            if(distanceToTarget < 1)
            {
                _npcAi.ChangeState(NpcState.idle);
                return;
            }

            //到达一个路径点
            var distanceToPathPoint = Vector3.DistanceSquared(_npcAi._npc._position, _currentPathTarget);
            if(distanceToPathPoint < 1 && _currentPathIndex < _currentPathList.Count - 1)
            {
                _currentPathTarget = _currentPathList[++_currentPathIndex];
                return;
            }

            //向目标点移动
            var directionToPathTarget = _currentPathTarget - _npcAi._npc._position;
            directionToPathTarget.Y = 0;
            directionToPathTarget = Vector3.Normalize(directionToPathTarget);
            var moveDelta = directionToPathTarget * _moveSpeed * MMORPG_SERVER.Time.Timer.deltaTime;
            _npcAi._npc._position += moveDelta;

            //向目标点旋转
            float yawRadians = (float)Math.Atan2(directionToPathTarget.X, directionToPathTarget.Z);
            float yawDeg = yawRadians * (180 / (float)Math.PI);
            _npcAi._npc._rotationY = yawDeg;
        }
    }
}
