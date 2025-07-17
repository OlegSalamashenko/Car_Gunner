using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

[RequireComponent(typeof(EnemyAnimator))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float activationDistance = 20f;
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 1f;

    private Transform _carTarget;
    private IObjectPool<Enemy> _pool;
    private EnemyAnimator _enemyAnimator;

    private bool _isActive;
    private bool _isDead;
    private int _currentHealth;

    private CancellationTokenSource _cts;

    public void Init(Transform car, IObjectPool<Enemy> pool)
    {
        _carTarget = car;
        _pool = pool;

        _isActive = false;
        _isDead = false;
        _currentHealth = maxHealth;

        if (_enemyAnimator == null)
            _enemyAnimator = GetComponent<EnemyAnimator>();

        gameObject.SetActive(true);

        _enemyAnimator.SetRunning(false);

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
                    _isActive = true;

                if (_isActive)
                {
                    if (distance > attackDistance)
                    {
                        _enemyAnimator.SetRunning(true);

                        Vector3 dir = (_carTarget.position - transform.position).normalized;
                        transform.position += dir * (speed * Time.deltaTime);
                        transform.LookAt(_carTarget);
                    }
                    else
                    {
                        _enemyAnimator.SetRunning(false);
                        _enemyAnimator.PlayPunch();

                        if (_carTarget.TryGetComponent<CarHealth>(out var carHealth))
                        {
                            carHealth.TakeDamage(damage);
                        }

                        await UniTask.Delay((int)(attackCooldown * 1000), cancellationToken: token);
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        if (_isDead && _pool != null)
            Release();
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

    private void Release()
    {
        _enemyAnimator.SetRunning(false);
        _cts?.Cancel();
        _pool?.Release(this);
    }

    private void OnDisable()
    {
        _cts?.Cancel();
    }
}
