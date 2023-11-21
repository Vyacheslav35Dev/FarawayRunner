using UnityEngine;
using Zenject;

public class LoadingInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Debug.Log("LoadingInstaller :: InstallBindings");
    }
}