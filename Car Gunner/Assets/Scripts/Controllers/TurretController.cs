using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

public class TurretController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretBase;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private string bulletAddress = "Bullet";
    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private float fireRate = 0.25f;

    private GameObject _bulletPrefab;
    private IObjectPool<Bullet> _bulletPool;

    private float _lastFireTime;

    private async void Start()
    {
        await LoadBulletPrefabAsync();

        _bulletPool = new ObjectPool<Bullet>(
            CreateBullet,
            bullet => bullet.gameObject.SetActive(true),
            bullet => bullet.gameObject.SetActive(false),
            bullet => Destroy(bullet.gameObject),
            true, 10, 100);
    }

    private void Update()
    {
        UpdateAim();
        TryFire();
    }

    private async UniTask LoadBulletPrefabAsync()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(bulletAddress);
        _bulletPrefab = await handle.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    private Bullet CreateBullet()
    {
        var obj = Instantiate(_bulletPrefab, muzzlePoint.position, Quaternion.identity);
        return obj.GetComponent<Bullet>();
    }

    private void UpdateAim()
    {
        if (!TryGetAimTarget(out Vector3 target)) return;

        Vector3 direction = target - turretBase.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            turretBase.rotation = Quaternion.Euler(-90f, rotation.eulerAngles.y, 0f);
        }
    }

    private void TryFire()
    {
        if (_bulletPrefab == null || !Input.GetMouseButton(0)) return;

        if (Time.time - _lastFireTime >= fireRate)
        {
            _lastFireTime = Time.time;
            FireAsync().Forget();
        }
    }

    private async UniTaskVoid FireAsync()
    {
        if (!TryGetAimTarget(out Vector3 target)) return;

        Vector3 direction = (target - muzzlePoint.position).normalized;

        var bullet = _bulletPool.Get();
        bullet.transform.SetPositionAndRotation(muzzlePoint.position, Quaternion.LookRotation(direction));
        bullet.Init(direction, _bulletPool);

        await UniTask.Yield();
    }

    private bool TryGetAimTarget(out Vector3 target)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask))
        {
            target = hit.collider.CompareTag("Enemy") ? hit.collider.bounds.center : hit.point;
            return true;
        }

        target = default;
        return false;
    }
}
