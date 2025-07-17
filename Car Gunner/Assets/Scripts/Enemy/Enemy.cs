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
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1.2f; 
    [SerializeField] private LayerMask carLayer;

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
                {
                    _isActive = true;
                    _enemyAnimator.SetRunning(true);
                }

                if (_isActive)
                {
                    if (distance > attackDistance)
                    {
                        _enemyAnimator.SetRunning(true);

                        Collider carCollider = _carTarget.GetComponent<Collider>();
                        Vector3 targetPos;
                        Vector3 lookAtPoint;

                        if (carCollider != null)
                        {
                            Vector3 closestPoint = carCollider.ClosestPoint(transform.position);
                            Vector3 dirToCar = (closestPoint - transform.position).normalized;
                            targetPos = closestPoint - dirToCar * attackDistance;
                            lookAtPoint = closestPoint;
                        }
                        else
                        {
                            Vector3 dir = (_carTarget.position - transform.position).normalized;
                            targetPos = _carTarget.position - dir * attackDistance;
                            lookAtPoint = _carTarget.position;
                        }

                        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                        transform.LookAt(lookAtPoint);
                    }
                    else
                    {
                        _enemyAnimator.SetRunning(false);
                        _enemyAnimator.PlayPunch();

                        await UniTask.Delay((int)(attackCooldown * 1000), cancellationToken: token);
                    }
                }
                else
                {
                    _enemyAnimator.SetRunning(false);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
            // отмена - ничего делать не нужно
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

    public void ApplyDamageEvent()
    {
        if (_isDead || attackPoint == null) return;

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, carLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<CarHealth>(out var carHealth))
            {
                carHealth.TakeDamage(damage);
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    private void OnDisable()
    {
        _cts?.Cancel();
    }
}
