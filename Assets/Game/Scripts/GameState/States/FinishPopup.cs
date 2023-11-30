using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.GameState.States
{
    public class FinishPopup : MonoBehaviour
    {
        [SerializeField]
        private Button _rerunBtn;

        private Action OnCallback;

        public void Init(Action callback)
        {
            OnCallback = callback;
        }

        private void OnEnable()
        {
            _rerunBtn.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _rerunBtn.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            OnCallback?.Invoke();
        }
    }
}