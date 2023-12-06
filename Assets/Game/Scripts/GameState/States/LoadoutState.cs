using Cysharp.Threading.Tasks;
using Game.Scripts.GameState.GameManager;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using YG;

namespace Game.Scripts.GameState.States
{
    public class LoadoutState : State
    {
        [SerializeField]
        private AssetReference _characterObjectReference;

        [Header("Root Ui Canvas")]
        [SerializeField]
        private Canvas _rootCanvas;
        
        [Header("Start Button")]
        [SerializeField] 
        private Button _startButton;
        
        [SerializeField]
        private GameObject _loadingCharPosition;
        
        [SerializeField]
        private TMP_Text _maxScoreText;
        
        [SerializeField] 
        private GameObject _rootEnv;
        
        private GameObject _characterObject;
        
        public override async UniTask Enter(State from)
        {
            _rootEnv.gameObject.SetActive(true);
            _rootCanvas.gameObject.SetActive(true);

            var maxScore = YandexGame.savesData.score;
            _maxScoreText.text = maxScore > 0 ? $"Твои максимальные очки : {maxScore}" : "";
            
            _characterObject = await Addressables.InstantiateAsync(_characterObjectReference, _loadingCharPosition.gameObject.transform);
            _startButton.onClick.AddListener(StartGame);
            _loadingCharPosition.SetActive(true);
        }

        public override async UniTask Exit(State to)
        {
            Destroy(_characterObject);
            Addressables.ReleaseInstance(_characterObject);
            _startButton.onClick.RemoveListener(StartGame);
            
            _rootCanvas.gameObject.SetActive(false);
            _loadingCharPosition.SetActive(false);
        }

        public override void Tick()
        {
           
        }

        private void StartGame()
        {
            _stateManager.SwitchState(StateType.Game);
        }
        
        public override StateType GetStateType()
        {
            return StateType.Lodout;
        }
    }
}