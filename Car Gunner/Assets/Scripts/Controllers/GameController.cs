using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

public class GameController : MonoBehaviour
{
    private CarMover _carMover;
    private CameraService _cameraService;
    private bool _gameStarted = false;

    [Inject]
    public void Construct(CarMover carMover, CameraService cameraService)
    {
        _carMover = carMover;
        _cameraService = cameraService;
    }

    private void Start()
    {
        _cameraService.ResetCamera();
    }

    private void Update()
    {
        if (!_gameStarted && Input.GetMouseButtonDown(0))
        {
            StartGameAsync().Forget();
        }
    }

    private async UniTaskVoid StartGameAsync()
    {
        _gameStarted = true;

        _cameraService.LerpToFollow();
        await UniTask.Delay(300);
        _carMover.StartMoving();
    }

}