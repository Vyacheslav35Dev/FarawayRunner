using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
public class AddressableAssetService
{
    private readonly Dictionary<string, AsyncOperationHandle> _cacheHandles = new Dictionary<string, AsyncOperationHandle>();
    private readonly Dictionary<string, SceneInstance> _cacheSceneHandles = new Dictionary<string, SceneInstance>();
    private bool _initialized;

    public async UniTask Initialize(CancellationToken cancellationToken = default)
    {
        try
        {
            await Addressables.InitializeAsync().WithCancellation(cancellationToken:cancellationToken);
            _initialized = true;
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"{nameof(AddressableAssetService)}]::Initialize operation canceled");
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(AddressableAssetService)}]::Initialize error {e}");
            throw;
        }
    }

    public async UniTask<SceneInstance> LoadScene(string key, LoadSceneMode sceneMode = LoadSceneMode.Single,
        bool activeOnLoad = true,
        CancellationToken cancellationToken = default)
    {
        CheckInitialized();
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException($"{nameof(AddressableAssetService)}]::LoadScene error key null {nameof(key)}");
        }

        try
        {
            var loadedScene = await Addressables.LoadSceneAsync(key, sceneMode, activateOnLoad:activeOnLoad)
                .WithCancellation(cancellationToken);
            _cacheSceneHandles.TryAdd(key, loadedScene);
           return loadedScene;
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"{nameof(AddressableAssetService)}]::LoadScene operation canceled {key}");
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(AddressableAssetService)}]::LoadScene error {e}");
            throw;
        }
    }

    //Work only in mode additive
    public async UniTask UnloadScene(string key)
    {
        CheckInitialized();
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }
        
        try
        {
            _cacheSceneHandles.TryGetValue(key, out SceneInstance loadedScene);
            await Addressables.UnloadSceneAsync(loadedScene);
            _cacheSceneHandles.Remove(key);
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"{nameof(AddressableAssetService)}]::UnloadScene operation canceled {key}");
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(AddressableAssetService)}]::UnloadScene error {e}");
            throw;
        }
    }

    public async UniTask<T> Load<T>(string key, CancellationToken cancellationToken = default,
        IProgress<float> progress = null, Action<LoadingProgress> onProgressExt = null) where T : class
    {
        CheckInitialized();
        try
        {
            var handler = Addressables.LoadAssetAsync<T>(key);
            while (!handler.IsDone)
            {
                var status = handler.GetDownloadStatus();
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report(status.Percent);
                onProgressExt?.Invoke(new LoadingProgress(status.Percent, status.DownloadedBytes, status.TotalBytes));
                await UniTask.Yield();
            }

            if (handler.Status == AsyncOperationStatus.Failed)
            {
                throw new LoadAssetEsception($"{nameof(AddressableAssetService)}]::Load operation error not found bundle {key}");
            }
            
            _cacheHandles.TryAdd(key, handler);
            
            return handler.Result;
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"{nameof(AddressableAssetService)}]::Load operation canceled {key}");
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(AddressableAssetService)}]::Load operation exception {e}");
            throw;
        }
    }
    
    public async UniTask<T> Load<T>(AssetReference assetReference, CancellationToken cancellationToken = default,
        IProgress<float> progress = null, Action<LoadingProgress> onProgressExt = null) where T : class
    {
        CheckInitialized();
        try
        {
            var handler = Addressables.LoadAssetAsync<T>(assetReference);
            while (!handler.IsDone)
            {
                var status = handler.GetDownloadStatus();
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report(status.Percent);
                onProgressExt?.Invoke(new LoadingProgress(status.Percent, status.DownloadedBytes, status.TotalBytes));
                await UniTask.Yield();
            }

            if (handler.Status == AsyncOperationStatus.Failed)
            {
                throw new LoadAssetEsception($"{nameof(AddressableAssetService)}]::Load operation error not found bundle {assetReference}");
            }
            
            _cacheHandles.TryAdd(assetReference.AssetGUID, handler);
            
            return handler.Result;
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"{nameof(AddressableAssetService)}]::Load operation canceled {assetReference}");
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(AddressableAssetService)}]::Load operation exception {e}");
            throw;
        }
    }

    public bool Unload(string key)
    {
        if (!_cacheHandles.TryGetValue(key, out var handle))
        {
            return false;
        }
        
        try
        {
            Addressables.Release(handle);
            _cacheHandles.Remove(key);
        }
        catch (Exception e)
        {
            Debug.LogError($"[{nameof(AddressableAssetService)}] Fail unload asset by key {key} {e}");
            return false;
        }

        return true;
    }

    private void CheckInitialized([CallerMemberName] string methodName = "")
    {
        if (!_initialized)
        {
            throw new InvalidOperationException($"{nameof(AddressableAssetService)}] need to initialize first");
        }
    }
    
    public GameObject GetPrefab(string key)
    {
        if (_cacheHandles.TryGetValue(key, out var handle))
        {
            return (GameObject)handle.Result;
        }

        return null;
    }
}
