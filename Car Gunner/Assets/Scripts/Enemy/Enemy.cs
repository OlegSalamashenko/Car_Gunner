using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

[RequireComponent(typeof(EnemyAnimator))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float activationDistance = 20f;
    [SerializeField] private float offsetDistance = 1.0f;
    [SerializeField] private float attackDistance  = 1.2f;
    [SerializeField] private float attackCooldown  = 1f;
    [SerializeField] private int damage = 1;

    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private LayerMask carLayer;
    
    [Header("Death")]
    [SerializeField] private string deathBallsAddress = "DeathBalls";           
    [SerializeField] private int ballsCount = 4;           
    [SerializeField] private float scatterStrength = 2f;   

    private GameObject _deathBallsPrefab;
    private Transform _carTarget;
    private EnemyAnimator _enemyAnimator;
    private IObjectPool<Enemy> _pool;

    private bool _isActive;
    private bool _isDead;
    private int  _currentHealth;
    private CancellationTokenSource _cts;


    private async void Start() => await LoadDeathBallsAsync();

    private async UniTask LoadDeathBallsAsync()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(deathBallsAddress);
        _deathBallsPrefab = await handle.ToUniTask();
    }


    public void Init(Transform car, IObjectPool<Enemy> pool)
    {
        _carTarget    = car;
        _pool         = pool;
        _isDead       = false;
        _isActive     = false;
        _currentHealth= maxHealth;

        if (_enemyAnimator == null)
            _enemyAnimator = GetComponent<EnemyAnimator>();

        _enemyAnimator.SetRunning(false);
        gameObject.SetActive(true);

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        StartAI(_cts.Token).Forget();
    }

    private async UniTaskVoid StartAI(CancellationToken token)
    {
        try
        {
            while (!_isDead && !token.IsCancellationRequested)
            {
                if (_carTarget == null) break;

                float distToCar = Vector3.Distance(transform.position, _carTarget.position);
                
                if (!_isActive && distToCar < activationDistance)
                {
                    _isActive = true;
                    _enemyAnimator.SetRunning(true);
                }

                if (_isActive)
                {
                    Collider carCol = _carTarget.GetComponent<Collider>();
                    Vector3 surfacePoint = carCol != null
                        ? carCol.ClosestPoint(transform.position)
                        : _carTarget.position;
                    
                    Vector3 dir = (surfacePoint - transform.position).normalized;
                    
                    Vector3 stopPoint = surfacePoint - dir * offsetDistance;

                    float distToStopPoint = Vector3.Distance(transform.position, stopPoint);

                    if (distToStopPoint > 0.05f)
                    {
                        _enemyAnimator.SetRunning(true);
                        transform.position = Vector3.MoveTowards(transform.position, stopPoint, speed * Time.deltaTime);
                        transform.LookAt(surfacePoint);
                    }
                    else
                    {
                        float distForAttack = Vector3.Distance(transform.position, surfacePoint);
                        if (distForAttack <= attackDistance)
                        {
                            _enemyAnimator.SetRunning(false);
                            _enemyAnimator.PlayPunch();
                            await UniTask.Delay((int)(attackCooldown * 1000), cancellationToken: token);
                        }
                        else
                        {
                            _enemyAnimator.SetRunning(true);
                            transform.position = Vector3.MoveTowards(transform.position, stopPoint, speed * Time.deltaTime);
                            transform.LookAt(surfacePoint);
                        }
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException) { }

        if (_isDead)
        {
            SpawnDeathBalls();
            _pool?.Release(this);
        }
    }
    private void SpawnDeathBalls()
    {
        if (_deathBallsPrefab == null) return;

        for (int i = 0; i < ballsCount; i++)
        {
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * 0.2f;
            var ball = Instantiate(_deathBallsPrefab, pos, UnityEngine.Random.rotation);
            if (ball.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 force = (Vector3.up * UnityEngine.Random.Range(0.5f, 1.5f)  
                                 + UnityEngine.Random.onUnitSphere * 0.5f)
                                * scatterStrength;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (_isDead) return;
        _currentHealth -= amount;
        if (_currentHealth <= 0) _isDead = true;
    }

    private void Release()
    {
        _enemyAnimator.SetRunning(false);
        _cts?.Cancel();
        _pool?.Release(this);
    }

    public void ApplyDamageEvent()
    {
        if (_isDead || attackPoint == null) return;
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, carLayer);
        foreach (var h in hits)
            if (h.TryGetComponent<CarHealth>(out var ch))
            {
                ch.TakeDamage(damage);
                break;
            }
    }

    private void OnDisable() => _cts?.Cancel();
}
