using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GameController gameController;
    [SerializeField] private CarMover carMover;
    [SerializeField] private CameraService cameraService;

    public override void InstallBindings()
    {
        Container.Bind<GameController>().FromInstance(gameController).AsSingle();
        Container.Bind<CarMover>().FromInstance(carMover).AsSingle();
        Container.Bind<CameraService>().FromInstance(cameraService).AsSingle();
    }
}
