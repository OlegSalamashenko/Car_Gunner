using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private int health = 3;
    [SerializeField] private float activationDistance = 20f;
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 1f;

    private Transform _carTarget;
    private bool _isActive = false;
    private bool _isDead = false;

    public void Init(Transform car)
    {
        _carTarget = car;
        StartAI().Forget();
    }

    private async UniTaskVoid StartAI()
    {
        while (!_isDead)
        {
            if (!_isActive)
            {
                float dist = Vector3.Distance(transform.position, _carTarget.position);
                if (dist < activationDistance)
                    _isActive = true;
            }

            if (_isActive)
            {
                float dist = Vector3.Distance(transform.position, _carTarget.position);
                if (dist > attackDistance)
                {
                    Vector3 dir = (_carTarget.position - transform.position).normalized;
                    transform.position += dir * speed * Time.deltaTime;
                    transform.LookAt(_carTarget);
                }
                else
                {
                    await Attack();
                    await UniTask.Delay(TimeSpan.FromSeconds(attackCooldown));
                }
            }

            await UniTask.Yield();
        }
    }

    private async UniTask Attack()
    {
        if (_carTarget.TryGetComponent<CarHealth>(out var carHealth))
        {
            carHealth.TakeDamage(damage);
        }
    }

    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        Destroy(gameObject);
    }
}
