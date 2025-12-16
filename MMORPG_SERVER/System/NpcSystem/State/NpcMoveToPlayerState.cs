using MMORPG_SERVER.System.AStarSystem;
using MMORPG_SERVER.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.NpcSystem.State
{
    //走向玩家状态
    public class NpcMoveToPlayerState : IState
    {
        private NpcAi _npcAi;

        private Vector3 _currentTarget;

        private List<Vector3> _pathList = new();

        private int _currentIndex;

        private Vector3 _currentPathTarget;

        private float _moveSpeed = 3f;

        public NpcMoveToPlayerState(NpcAi npcAi)
        {
            _npcAi = npcAi;
        }

        public bool Condition(IState newState)
        {
            return true;
        }

        public async void Enter()
        {
            _npcAi._npc._stateId = (int)NpcState.move;
            _currentTarget = _npcAi._currentTarget;
            _currentIndex = 0;
            _pathList = await AStarManager.Instance.GetAStarPath(_npcAi._npc._position, _currentTarget);
            _currentPathTarget = _pathList[_currentIndex];
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
            //到达目的地
            var distanceToTarget = Vector3.DistanceSquared(_npcAi._npc._position, _currentTarget);
            if(distanceToTarget < 9f)
            {
                _npcAi._currentTarget = Vector3.Zero;
                _npcAi.ChangeState(NpcState.idle);
            }

            //到达路径点
            var distanceToPathTarget = Vector3.DistanceSquared(_npcAi._npc._position, _currentPathTarget);
            if(distanceToPathTarget < 1f && _currentIndex < _pathList.Count - 1)
            {
                _currentPathTarget = _pathList[++_currentIndex];
                return;
            }

            //move
            var direction = _currentTarget - _npcAi._npc._position;
            direction.Y = 0;
            var moveDelta = 
                _moveSpeed * Vector3.Normalize(direction) * MMORPG_SERVER.Time.Timer.deltaTime;
            _npcAi._npc._position += moveDelta;

            //rotate
            //向目标点旋转
            float yawRadians = (float)Math.Atan2(direction.X, direction.Z);
            float yawDeg = yawRadians * (180 / (float)Math.PI);
            _npcAi._npc._rotationY = yawDeg;
        }
    }
}
