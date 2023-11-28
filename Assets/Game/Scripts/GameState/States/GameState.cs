using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.GameState.GameManager;
using Game.Scripts.Infra.Music;
using Game.Scripts.Tracks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.GameState.States
{
    public class GameState : State
    {
        static int s_DeadHash = Animator.StringToHash("Dead");
        
        [Header("UI")]
        [SerializeField] 
        private Canvas _rootCanvas;
        
        [SerializeField] 
        private GameObject _waitingPopup;
        
        [SerializeField] 
        private TMP_Text _coinText;
        
        [SerializeField]
        private AudioClip _gameTheme;

        [SerializeField] 
        private TrackManager _trackManager;
        
        private bool m_Finished = true;
        
        public override async UniTask Enter(State from)
        {
            _rootCanvas.gameObject.SetActive(true);
            
            if (MusicPlayer.instance.GetStem(0) != _gameTheme)
            {
                MusicPlayer.instance.SetStem(0, _gameTheme);
                await MusicPlayer.instance.RestartAllStems();
            }

            m_Finished = false;
            
            await _trackManager.Begin();
            
            _waitingPopup.gameObject.SetActive(false);
        }

        public override async UniTask Exit(State to)
        {
            _waitingPopup.gameObject.SetActive(true);
            _trackManager.gameObject.SetActive(false);
        }

        public override void Tick()
        {
            if (_trackManager.isLoaded)
            {
                if (_trackManager.CharacterController.CurrentLife <= 0)
                {
                    _trackManager.CharacterController.character.animator.SetBool(s_DeadHash, true);
                    WaitForGameOver().Forget();
                }
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            _coinText.text = _trackManager.CharacterController.Coins.ToString();
        }

        public override StateType GetStateType()
        {
            return StateType.Game;
        }
        
        private async UniTask WaitForGameOver()
        {
            m_Finished = true;
            _trackManager.StopMove();
            
            Shader.SetGlobalFloat("_BlinkingValue", 0.0f);
            await UniTask.Delay(2000);
            _stateManager.SwitchState(StateType.Gameover);
        }
    }
}