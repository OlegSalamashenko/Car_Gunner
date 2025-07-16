using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Zenject;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform carTarget;

    [Header("Wave Settings")]
    [SerializeField] private int enemiesPerWave = 4;
    [SerializeField] private float waveInterval = 3f;

    [Header("Spawn Position Settings")]
    [SerializeField] private float spawnDistanceAhead = 50f;
    [SerializeField] private float spawnRadiusSide = 15f;
    [SerializeField] private float levelLength = 400f;

    private bool _spawningActive;

    public void StartSpawning()
    {
        _spawningActive = true;
        SpawnLoop().Forget();
    }
    public void StopSpawning() => _spawningActive = false;
    
    private async UniTaskVoid SpawnLoop()
    {
        while (_spawningActive)
        {
            SpawnEnemyWave();
            await UniTask.Delay(TimeSpan.FromSeconds(waveInterval));
        }
    }

    private void SpawnEnemyWave()
    {
        if (!carTarget) return;

        float carZ = carTarget.position.z;
        if (carZ >= levelLength)
        {
            _spawningActive = false;
            return;
        }

        for (int i = 0; i < enemiesPerWave; i++)
        {
            Vector3 spawnPos = new Vector3(
                carTarget.position.x + UnityEngine.Random.Range(-spawnRadiusSide, spawnRadiusSide),
                carTarget.position.y,
                carTarget.position.z + UnityEngine.Random.Range(spawnDistanceAhead * 0.5f, spawnDistanceAhead)
            );

            var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.GetComponent<Enemy>().Init(carTarget);
        }
    }
}