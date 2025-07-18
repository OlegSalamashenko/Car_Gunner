using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

[RequireComponent(typeof(EnemyAnimator))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float activationDistance = 20f;
    [SerializeField] private float offsetDistance = 1.0f;
    [SerializeField] private float attackDistance = 1.2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int damage = 1;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private LayerMask carLayer;

    [Header("Death")]
    [SerializeField] private AssetReferenceGameObject deathBallsAddress;
    [SerializeField] private int ballsCount = 4;
    [SerializeField] private float scatterStrength = 2f;

    private GameObject _deathBallsPrefab;
    private Transform _carTarget;
    private EnemyAnimator _animator;
    private IObjectPool<Enemy> _pool;

    private CancellationTokenSource _cts;
    private int _currentHealth;
    private bool _isActive;
    private bool _isDead;

    private async void Start() => await LoadDeathEffectAsync();

    private async UniTask LoadDeathEffectAsync()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(deathBallsAddress);
        _deathBallsPrefab = await handle.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public void Init(Transform car, IObjectPool<Enemy> pool)
    {
        _carTarget = car;
        _pool = pool;

        _isDead = false;
        _isActive = false;
        _currentHealth = maxHealth;

        _animator ??= GetComponent<EnemyAnimator>();
        _animator.SetRunning(false);

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

                float distance = Vector3.Distance(transform.position, _carTarget.position);

                if (!_isActive && distance < activationDistance)
                {
                    _isActive = true;
                    _animator.SetRunning(true);
                }

                if (_isActive)
                {
                    Vector3 surfacePoint = GetTargetSurfacePoint();
                    Vector3 direction = (surfacePoint - transform.position).normalized;
                    Vector3 stopPoint = surfacePoint - direction * offsetDistance;

                    float distanceToStop = Vector3.Distance(transform.position, stopPoint);

                    if (distanceToStop > 0.05f)
                    {
                        MoveTowards(stopPoint, surfacePoint);
                    }
                    else
                    {
                        if (Vector3.Distance(transform.position, surfacePoint) <= attackDistance)
                        {
                            _animator.SetRunning(false);
                            _animator.PlayPunch();
                            await UniTask.Delay((int)(attackCooldown * 1000), cancellationToken: token);
                        }
                        else
                        {
                            MoveTowards(stopPoint, surfacePoint);
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

    private void MoveTowards(Vector3 stopPoint, Vector3 lookAt)
    {
        _animator.SetRunning(true);
        transform.position = Vector3.MoveTowards(transform.position, stopPoint, speed * Time.deltaTime);
        transform.LookAt(lookAt);
    }

    private Vector3 GetTargetSurfacePoint()
    {
        Collider col = _carTarget.GetComponent<Collider>();
        return col != null ? col.ClosestPoint(transform.position) : _carTarget.position;
    }

    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            _isDead = true;
        }
    }

    public void ApplyDamageEvent()
    {
        if (_isDead || attackPoint == null) return;

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, carLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<CarHealth>(out var car))
            {
                car.TakeDamage(damage);
                break;
            }
        }
    }

    private void SpawnDeathBalls()
    {
        if (_deathBallsPrefab == null) return;

        for (int i = 0; i < ballsCount; i++)
        {
            Vector3 position = transform.position + UnityEngine.Random.insideUnitSphere * 0.2f;
            GameObject ball = Instantiate(_deathBallsPrefab, position, UnityEngine.Random.rotation);

            if (ball.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 force = (Vector3.up * UnityEngine.Random.Range(0.5f, 1.5f) +
                                 UnityEngine.Random.onUnitSphere * 0.5f) * scatterStrength;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }

    private void OnDisable() => _cts?.Cancel();
}
