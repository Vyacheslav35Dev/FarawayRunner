using Cysharp.Threading.Tasks;
using Game.Scripts.GameState.States;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameState.GameManager
{
    public abstract class State : MonoBehaviour
    {
        [Inject] 
        protected GameManager _stateManager;

        public abstract UniTask Enter(State from);
        public abstract UniTask Exit(State to);
        public abstract void Tick();
        public abstract StateType GetStateType();
    }
}