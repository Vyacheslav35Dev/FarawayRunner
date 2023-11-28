using UnityEngine;
using Zenject;

namespace Game.Scripts.GameState
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private GameManager.GameManager _gameManager;
        public override void InstallBindings()
        {
            Container.Bind<GameManager.GameManager>().FromInstance(_gameManager).AsSingle().NonLazy();
        }
    }
}