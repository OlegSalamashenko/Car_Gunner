using UnityEngine;
using Cysharp.Threading.Tasks;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private int health = 3;
    [SerializeField] private float activationDistance = 20f;
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 1f;

    private Transform _carTarget;
    private bool _isActive;
    private bool _isDead;

    public void Init(Transform car)
    {
        _carTarget = car;
        _isActive = false;
        _isDead = false;

        StartAI().Forget();
    }

    private async UniTaskVoid StartAI()
    {
        var token = this.GetCancellationTokenOnDestroy();

        while (!_isDead)
        {
            if (_carTarget == null) return;

            float distance = Vector3.Distance(transform.position, _carTarget.position);

            if (!_isActive && distance < activationDistance)
                _isActive = true;

            if (_isActive)
            {
                if (distance > attackDistance)
                {
                    Vector3 dir = (_carTarget.position - transform.position).normalized;
                    transform.position += dir * speed * Time.deltaTime;
                    transform.LookAt(_carTarget);
                }
                else
                {
                    if (_carTarget.TryGetComponent<CarHealth>(out var carHealth))
                    {
                        carHealth.TakeDamage(damage);
                    }

                    await UniTask.Delay((int)(attackCooldown * 1000), cancellationToken: token);
                }
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        Destroy(gameObject);
    }

    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        health -= amount;
        if (health <= 0)
        {
            _isDead = true;
        }
    }
}