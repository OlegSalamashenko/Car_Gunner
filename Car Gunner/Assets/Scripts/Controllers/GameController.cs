using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private CarMover _carMover;
    private CameraService _cameraService;
    private EnemySpawner _enemySpawner;
    private FinishTrigger _finishTrigger;
    private CarHealth _carHealth;

    private bool _gameStarted;
    private bool _gameEnded;

    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TextMeshProUGUI resultText;

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
        _finishTrigger.OnFinish += OnFinish;
        _carHealth.OnCarDestroyed += OnCarDestroyed;

        gameOverUI.SetActive(false);
    }

    private void OnDestroy()
    {
        _finishTrigger.OnFinish -= OnFinish;
        _carHealth.OnCarDestroyed -= OnCarDestroyed;
    }

    private async void OnFinish()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        _enemySpawner.StopSpawning();
        _carMover.StopMoving();

        ShowResult("You Win");

        await WaitForRestartTap();
    }

    private async void OnCarDestroyed()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        _enemySpawner.StopSpawning();
        _carMover.StopMoving();

        ShowResult("You Lose");

        await WaitForRestartTap();
    }

    private void ShowResult(string message)
    {
        resultText.text = message;
        gameOverUI.SetActive(true);
    }

    private async UniTask WaitForRestartTap()
    {
        while (!Input.GetMouseButtonDown(0))
        {
            await UniTask.Yield();
        }
        RestartGame();
    }

    private void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
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
        _enemySpawner.StartSpawning();
    }
}
