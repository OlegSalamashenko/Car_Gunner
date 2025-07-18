using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(TrailRenderer))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float speed = 30f;
    [SerializeField] private int damage = 1;

    private Vector3 direction;
    private IObjectPool<Bullet> pool;
    private TrailRenderer trail;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

    public void Init(Vector3 newDirection, IObjectPool<Bullet> bulletPool)
    {
        direction = newDirection;
        pool = bulletPool;

        gameObject.SetActive(true);
        trail.Clear();

        CancelInvoke(nameof(Release));
        Invoke(nameof(Release), lifetime);
    }

    private void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);
        }

        Release();
    }

    private void Release()
    {
        CancelInvoke(nameof(Release));
        trail.Clear();
        pool?.Release(this);
    }
}