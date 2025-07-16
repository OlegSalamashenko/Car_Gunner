using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform carTarget;
    [SerializeField] private float spawnDistanceAhead = 60f;
    [SerializeField] private float spawnRadiusSide = 15f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float levelLength = 400f;

    private bool _spawningActive = true;

    private void Start()
    {
        StartSpawning().Forget();
    }

    private async UniTaskVoid StartSpawning()
    {
        while (_spawningActive)
        {
            SpawnEnemyAhead();
            await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval));
        }
    }

    private void SpawnEnemyAhead()
    {
        if (!carTarget) return;

        float carZ = carTarget.position.z;

        if (carZ >= levelLength)
        {
            _spawningActive = false;
            return;
        }

        Vector3 spawnPos = new Vector3(
            carTarget.position.x + UnityEngine.Random.Range(-spawnRadiusSide, spawnRadiusSide),
            carTarget.position.y,
            carTarget.position.z + UnityEngine.Random.Range(spawnDistanceAhead * 0.5f, spawnDistanceAhead)
        );

        var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        enemy.GetComponent<Enemy>().Init(carTarget);
    }
}