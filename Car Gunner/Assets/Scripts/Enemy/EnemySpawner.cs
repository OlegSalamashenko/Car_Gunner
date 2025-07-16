using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Transform carTarget;
    [SerializeField] private float spawnDistanceAhead = 60f;
    [SerializeField] private float spawnRadiusSide = 10f;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float levelLength = 400f;

    [SerializeField] private int minEnemiesPerWave = 3;
    [SerializeField] private int maxEnemiesPerWave = 6;

    private bool _spawningActive;
    private IObjectPool<Enemy> _enemyPool;
    private List<Enemy> _activeEnemies = new();

    private void Awake()
    {
        _enemyPool = new ObjectPool<Enemy>(
            CreateEnemy,
            OnGetEnemy,
            OnReleaseEnemy,
            OnDestroyEnemy,
            collectionCheck: false, defaultCapacity: 10, maxSize: 100
        );
    }

    private Enemy CreateEnemy()
    {
        var enemy = Instantiate(enemyPrefab);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    private void OnGetEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(true);
        _activeEnemies.Add(enemy);
    }

    private void OnReleaseEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        _activeEnemies.Remove(enemy);
    }

    private void OnDestroyEnemy(Enemy enemy)
    {
        Destroy(enemy.gameObject);
    }

    public void StartSpawning()
    {
        if (_spawningActive) return;
        _spawningActive = true;
        SpawnWaves().Forget();
    }

    public void StopSpawning()
    {
        _spawningActive = false;

        foreach (var enemy in _activeEnemies.ToArray())
        {
            _enemyPool.Release(enemy);
        }
        _activeEnemies.Clear();
    }

    private async UniTaskVoid SpawnWaves()
    {
        while (_spawningActive)
        {
            SpawnEnemyWave();
            await UniTask.Delay(System.TimeSpan.FromSeconds(spawnInterval));
        }
    }

    private void SpawnEnemyWave()
    {
        if (!carTarget) return;
        if (carTarget.position.z >= levelLength)
        {
            StopSpawning();
            return;
        }

        int count = Random.Range(minEnemiesPerWave, maxEnemiesPerWave + 1);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = new Vector3(
                carTarget.position.x + Random.Range(-spawnRadiusSide, spawnRadiusSide),
                carTarget.position.y,
                carTarget.position.z + Random.Range(spawnDistanceAhead * 0.5f, spawnDistanceAhead)
            );

            var enemy = _enemyPool.Get();
            enemy.transform.position = spawnPos;
            enemy.Init(carTarget, _enemyPool);
        }
    }
}
