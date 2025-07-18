using System;
using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class EnemySpawner : MonoBehaviour
{
    [Header("Addressables")]
    [SerializeField] private AssetReferenceGameObject enemyAddress;

    [Header("Target & Spawn Settings")]
    [SerializeField] private Transform carTarget;
    [SerializeField] private float spawnDistanceAhead = 60f;
    [SerializeField] private float spawnRadiusSide = 10f;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float levelLength = 400f;

    [Header("Enemies Per Wave")]
    [SerializeField] private int minEnemiesPerWave = 3;
    [SerializeField] private int maxEnemiesPerWave = 4;

    private GameObject enemyPrefab;
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
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 100
        );
    }

    private async void Start()
    {
        await LoadEnemyPrefabAsync();
    }

    private async UniTask LoadEnemyPrefabAsync()
    {
        if (enemyAddress == null)
        {
            Debug.LogError("Enemy address is not assigned!");
            return;
        }

        enemyPrefab = await enemyAddress.LoadAssetAsync().ToUniTask();
    }

    public async void StartSpawning()
    {
        if (_spawningActive) return;

        // Ожидаем загрузки префаба
        while (enemyPrefab == null)
            await UniTask.Yield();

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

    private Enemy CreateEnemy()
    {
        var obj = Instantiate(enemyPrefab);
        obj.SetActive(false);
        return obj.GetComponent<Enemy>();
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

    private async UniTaskVoid SpawnWaves()
    {
        while (_spawningActive)
        {
            SpawnEnemyWave();
            await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval));
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

        int count = UnityEngine.Random.Range(minEnemiesPerWave, maxEnemiesPerWave + 1);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = new Vector3(
                carTarget.position.x + UnityEngine.Random.Range(-spawnRadiusSide, spawnRadiusSide),
                carTarget.position.y + 10f,
                carTarget.position.z + UnityEngine.Random.Range(spawnDistanceAhead * 0.5f, spawnDistanceAhead)
            );

            if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 20f))
            {
                spawnPos.y = hit.point.y;
            }

            var enemy = _enemyPool.Get();
            enemy.transform.position = spawnPos;
            enemy.Init(carTarget, _enemyPool);
        }
    }
}
