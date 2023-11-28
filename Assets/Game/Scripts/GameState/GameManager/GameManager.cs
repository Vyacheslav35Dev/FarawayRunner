using System.Collections.Generic;
using Game.Scripts.GameState.States;
using UnityEngine;

namespace Game.Scripts.GameState.GameManager
{
    public class GameManager : MonoBehaviour
    {
        private List<State> _stateStack = new List<State>();
        private Dictionary<StateType, State> _stateDict = new Dictionary<StateType, State>();
        
        [SerializeField]
        private State[] states;

        public State TopState
        {
            get 
            { 
                if (_stateStack.Count == 0) return null; 
                return _stateStack[_stateStack.Count - 1]; 
            }
        }
        
        public void Run()
        {
            _stateDict.Clear();

            if (states.Length == 0)
                return;

            for(int i = 0; i < states.Length; ++i)
            {
                _stateDict.Add(states[i].GetStateType(), states[i]);
            }

            _stateStack.Clear();

            PushState(states[0].GetStateType());
        }

        protected void Update()
        {
            if(_stateStack.Count > 0)
            {
                _stateStack[_stateStack.Count - 1].Tick();
            }
        }
        
        public void SwitchState(StateType newState)
        {
            State state = FindState(newState);
            if (state == null)
            {
                Debug.LogError("Can't find the state named " + newState);
                return;
            }

            _stateStack[_stateStack.Count - 1].Exit(state);
            state.Enter(_stateStack[_stateStack.Count - 1]);
            _stateStack.RemoveAt(_stateStack.Count - 1);
            _stateStack.Add(state);
        }

	    public State FindState(StateType stateType)
	    {
		    State state;
		    if (!_stateDict.TryGetValue(stateType, out state))
		    {
			    return null;
		    }

		    return state;
	    }

        public void PopState()
        {
            if(_stateStack.Count < 2)
            {
                Debug.LogError("Can't pop states, only one in stack.");
                return;
            }

            _stateStack[_stateStack.Count - 1].Exit(_stateStack[_stateStack.Count - 2]);
            _stateStack[_stateStack.Count - 2].Enter(_stateStack[_stateStack.Count - 2]);
            _stateStack.RemoveAt(_stateStack.Count - 1);
        }

        public void PushState(StateType stateType)
        {
            State state;
            if(!_stateDict.TryGetValue(stateType, out state))
            {
                Debug.LogError("Can't find the state named " + stateType);
                return;
            }

            if (_stateStack.Count > 0)
            {
                _stateStack[_stateStack.Count - 1].Exit(state);
                state.Enter(_stateStack[_stateStack.Count - 1]);
            }
            else
            {
                state.Enter(null);
            }
            _stateStack.Add(state);
        }
    }
}