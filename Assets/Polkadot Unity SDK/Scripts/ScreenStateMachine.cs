using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IScreenState
    {
        void EnterState();

        void ExitState();

        void UpdateState();
    }

    public abstract class ScreenStateMachine<TState, TSubState> : MonoBehaviour
        where TState : struct, Enum
        where TSubState : struct, Enum
    {
        public TState CurrentState { get; internal set; }

        internal IScreenState _currentState;
        internal IScreenState _currentSubState;

        internal Dictionary<TState, IScreenState> _stateDictionary;
        internal Dictionary<TState, Dictionary<TSubState, IScreenState>> _subStateDictionary;

        internal void Awake()
        {
            _stateDictionary = new();
            _subStateDictionary = new();
            InitializeStates();
        }

        protected abstract void InitializeStates();

        /// <summary>
        /// Change the screen state
        /// </summary>
        /// <param name="newScreenState"></param>
        internal void ChangeScreenState(TState newScreenState)
        {
            CurrentState = newScreenState;

            // exit current state if any
            _currentState?.ExitState();

            // change the state
            _currentState = _stateDictionary[newScreenState];

            // exit any active sub-state when changing the main state
            _currentSubState?.ExitState();
            _currentSubState = null;

            // enter current state
            _currentState.EnterState();
        }

        /// <summary>
        /// Change the sub state of the current screen state
        /// </summary>
        /// <param name="parentState"></param>
        /// <param name="newSubState"></param>
        internal void ChangeScreenSubState(TState parentState, TSubState newSubState)
        {
            if (_subStateDictionary.ContainsKey(parentState) && _subStateDictionary[parentState].ContainsKey(newSubState))
            {
                // exit current sub state if any
                _currentSubState?.ExitState();

                // change the sub state
                _currentSubState = _subStateDictionary[parentState][newSubState];

                // enter current sub state
                _currentSubState.EnterState();
            }
        }
    }
}