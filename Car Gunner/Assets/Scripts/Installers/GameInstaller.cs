using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GameController gameController;
    [SerializeField] private CarMover carMover;
    [SerializeField] private CameraService cameraService;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private FinishTrigger finishTrigger;
    [SerializeField] private CarHealth carHealth;

    public override void InstallBindings()
    {
        Container.Bind<GameController>().FromInstance(gameController).AsSingle();
        Container.Bind<CarMover>().FromInstance(carMover).AsSingle();
        Container.Bind<CameraService>().FromInstance(cameraService).AsSingle();
        Container.Bind<EnemySpawner>().FromInstance(enemySpawner).AsSingle();
        Container.Bind<FinishTrigger>().FromInstance(finishTrigger).AsSingle();
        Container.Bind<CarHealth>().FromInstance(carHealth).AsSingle();
    }
}
