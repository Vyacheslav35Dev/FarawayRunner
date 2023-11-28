using Game.Scripts.Character;
using Game.Scripts.Infra.Storage;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<PlayerPrefsStorage>().AsSingle().NonLazy();
        Container.Bind<Player>().FromNew().AsSingle().NonLazy();
    }
}