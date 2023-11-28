using Cysharp.Threading.Tasks;
using Game.Scripts.GameState.GameManager;

namespace Game.Scripts.GameState.States
{
    public class GameoverState : State
    {
        public override UniTask Enter(State from)
        {
            throw new System.NotImplementedException();
        }

        public override UniTask Exit(State to)
        {
            throw new System.NotImplementedException();
        }

        public override void Tick()
        {
            throw new System.NotImplementedException();
        }

        public override StateType GetStateType()
        {
            return StateType.Gameover;
        }
    }
}