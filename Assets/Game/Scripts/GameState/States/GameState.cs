using Cysharp.Threading.Tasks;
using Game.Scripts.GameState.GameManager;
using Game.Scripts.Infra.Music;
using Game.Scripts.Tracks;
using TMPro;
using UnityEngine;

namespace Game.Scripts.GameState.States
{
    public class GameState : State
    {
        static int s_DeadHash = Animator.StringToHash("Dead");
        
        [Header("UI")]
        [SerializeField] 
        private Canvas _rootCanvas;
        
        [SerializeField] 
        private TMP_Text _coinText;
        
        [SerializeField] 
        private TMP_Text _lifeCountText;
        
        [SerializeField]
        private AudioClip _gameTheme;

        [SerializeField] 
        private TrackManager _trackManager;
        
        [SerializeField] 
        private GameObject _rootUiBackground;
        
        private bool m_Finished;

        public override async UniTask Enter(State from)
        {
            m_Finished = false;
            _rootCanvas.gameObject.SetActive(true);
            
            if (MusicPlayer.instance.GetStem(0) != _gameTheme)
            {
                MusicPlayer.instance.SetStem(0, _gameTheme);
                await MusicPlayer.instance.RestartAllStems();
            }

            await _trackManager.Begin();
            _rootUiBackground.gameObject.SetActive(false);
        }

        public override async UniTask Exit(State to)
        {
            _trackManager.gameObject.SetActive(false);
            _rootCanvas.gameObject.SetActive(false);
        }

        public override void Tick()
        {
            if (_trackManager.isLoaded)
            {
                if (_trackManager.CharacterController.CurrentLife <= 0 && !m_Finished)
                {
                    WaitForGameOver().Forget();
                }
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            _coinText.text = _trackManager.CharacterController.Coins.ToString();
            _lifeCountText.text = $"ЖИЗНИ {_trackManager.CharacterController.CurrentLife}";
        }

        public override StateType GetStateType()
        {
            return StateType.Game;
        }
        
        private async UniTask WaitForGameOver()
        {
            if (m_Finished)
            {
                return;
            }
            _trackManager.CharacterController.character.animator.SetBool(s_DeadHash, true);
            
            m_Finished = true;
            _trackManager.StopMove();
            
            Shader.SetGlobalFloat("_BlinkingValue", 0.0f);
            
           _stateManager.SwitchState(StateType.Gameover);
           _trackManager.CharacterController.CurrentLife = _trackManager.CharacterController.maxLife;
           _trackManager.End();
        }
    }
}