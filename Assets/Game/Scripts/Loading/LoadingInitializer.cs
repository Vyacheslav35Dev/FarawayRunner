using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Character;
using Game.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Zenject;

namespace Game.Scripts.Loading
{
    public class LoadingInitializer : MonoBehaviour
    {
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private TMP_Text procCount;
        [SerializeField] private TMP_Text loadingVolumeText;
        [SerializeField] private GameObject sliderHeader;
        [SerializeField] private AnimationCurve _runningCurve;
    
        private Player _player;
    
        [Inject]
        private void Construct(Player player)
        {
            _player = player;
        }
    
        private async UniTaskVoid Start()
        {
            try
            {
                await InitServices(this.GetCancellationTokenOnDestroy());
                await Run();
            }
            catch (Exception e)
            {
                Debug.LogError($"LoadingSceneInitializer::Start exc {e}");
                throw;
            }
        }
    
        private async UniTask InitServices(CancellationToken cancellationToken = default)
        {
            try
            {
                await Addressables.InitializeAsync();
                await _player.Create();
            }
            catch (Exception e)
            {
                Debug.LogError($"LoadingInitializer::InitServices exc {e}");
                throw;
            }
        }

        private async UniTask Run(CancellationToken cancellationToken = default)
        {
            await RunSliderFromTo(0, 100, 3, cancellationToken);
            await Addressables.LoadSceneAsync(SceneType.Game.ToString());
        }
    
        private async UniTask RunSliderFromTo(int startValue, int endValue, float time, CancellationToken cancellationToken = default)
        {
            var lerpBetweenValuesData =
                new LerpBetweenValuesData(startValue, endValue, time, _runningCurve, SetSliderValue);
            await LerpBetweenValueInternalAsync(lerpBetweenValuesData, cancellationToken);
        }
    
        private void SetSliderValue(float sliderValue)
        {
            loadingSlider.value = (int) sliderValue;
            procCount.text = $"{(int)sliderValue} %";
        }
    
        private async UniTask LerpBetweenValueInternalAsync(LerpBetweenValuesData data, CancellationToken cancellationToken)
        {
            var progress = 0f;
            var currentValueRounded = 0f;

            while (progress < data.Duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress += Time.deltaTime;
                var progressPercent = Mathf.Clamp01(progress / data.Duration);
                var valueOnCurve = data.AnimationCurve.Evaluate(progressPercent);
                var currentValue = Mathf.Lerp(data.StartValue, data.EndValue, valueOnCurve);
                currentValueRounded = currentValue;
                data.OnLerp(currentValueRounded);

                await UniTask.Yield();
            }
            data.OnLerp(currentValueRounded);
        }
    }
}
