using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private TextMeshProUGUI _resultText;

    private CarMover _carMover;
    private CameraService _cameraService;
    private EnemySpawner _enemySpawner;
    private FinishTrigger _finishTrigger;
    private CarHealth _carHealth;

    private bool _gameStarted;
    private bool _gameEnded;

    [Inject]
    public void Construct(
        CarMover carMover,
        CameraService cameraService,
        EnemySpawner enemySpawner,
        FinishTrigger finishTrigger,
        CarHealth carHealth)
    {
        _carMover = carMover;
        _cameraService = cameraService;
        _enemySpawner = enemySpawner;
        _finishTrigger = finishTrigger;
        _carHealth = carHealth;
    }

    private void Start()
    {
        _cameraService.ResetCamera();

        _finishTrigger.OnFinish += HandleFinish;
        _carHealth.OnCarDestroyed += HandleCarDestroyed;

        _gameOverUI.SetActive(false);
    }

    private void OnDestroy()
    {
        _finishTrigger.OnFinish -= HandleFinish;
        _carHealth.OnCarDestroyed -= HandleCarDestroyed;
    }

    private async void HandleFinish()
    {
        await EndGame("You Win");
    }

    private async void HandleCarDestroyed()
    {
        await EndGame("You Lose");
    }

    private async UniTask EndGame(string message)
    {
        if (_gameEnded) return;
        _gameEnded = true;

        _enemySpawner.StopSpawning();
        _carMover.StopMoving();

        ShowResult(message);
        await WaitForRestartTap();
    }

    private void ShowResult(string message)
    {
        _resultText.text = message;
        _gameOverUI.SetActive(true);
    }

    private async UniTask WaitForRestartTap()
    {
        while (!Input.GetMouseButtonDown(0))
        {
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }

        RestartGame();
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        await UniTask.Delay(300, cancellationToken: this.GetCancellationTokenOnDestroy());

        _carMover.StartMoving();
        _enemySpawner.StartSpawning();
    }
}
