using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game.Scripts.GameState
{
    public class GameInitializer : MonoBehaviour
    {
        private GameManager.GameManager _gameManager;
        
        [Inject]
        private void Construct(GameManager.GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        
        void Start()
        {
           _gameManager.Run();
        }
    }
}
