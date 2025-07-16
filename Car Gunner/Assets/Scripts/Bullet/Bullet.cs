using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    private float _lifetime = 3f;
    private float _speed = 30f;
    private Vector3 _direction;
    private IObjectPool<Bullet> _pool;
    private int _damage = 1;

    private TrailRenderer _trail;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
    }

    public void Init(Vector3 direction, IObjectPool<Bullet> pool)
    {
        _direction = direction;
        _pool = pool;

        gameObject.SetActive(true);
        CancelInvoke();
        Invoke(nameof(Release), _lifetime);

        _trail.Clear();
    }

    private void Update()
    {
        transform.position += _direction * (_speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<Enemy>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(_damage);
            }
            Release();
        }
    }

    private void Release()
    {
        CancelInvoke();
        _trail.Clear();
        _pool?.Release(this);
    }
}