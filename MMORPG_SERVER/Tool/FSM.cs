using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Tool
{
    //实现FSM状态机

    public interface IState
    {
        bool Condition(IState newState);
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }

    //状态机基类
    public abstract class FSM<TState>  where TState : notnull
    {
        protected Dictionary<TState, IState> _stateDictionary = new();

        public IState _currentState;

        protected TState _currentStateId;

        public void AddState(TState stateId, IState state)
        {
            _stateDictionary.Add(stateId, state);
        }

        public void RemoveState(TState stateId)
        {
            _stateDictionary.Remove(stateId);
        }

        public void ChangeState(TState stateId)
        {
            if (!_stateDictionary.ContainsKey(stateId)) return;
            if (_currentState == _stateDictionary[stateId]) return;
            if (_currentState != null && !_currentState.Condition(_stateDictionary[stateId])) return;

            _currentState?.Exit();
            _currentStateId = stateId;
            _currentState = _stateDictionary[stateId];
            _currentState.Enter();
        }

        public virtual void Update()
        {
            _currentState?.Update();
        }

        public virtual void FixedUpdate()
        {
            _currentState?.FixedUpdate();
        }
    }
}
