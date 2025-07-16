using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class TurretController : MonoBehaviour
{
    [SerializeField] private Transform turretBase;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private string bulletAddress = "Bullet";
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private float fireRate = 0.25f;

    private GameObject bulletPrefab;
    private float _lastFireTime;
    private IObjectPool<Bullet> _bulletPool;

    private void Awake()
    {
        _bulletPool = new ObjectPool<Bullet>(
            CreateBullet,
            bullet => bullet.gameObject.SetActive(true),
            bullet => bullet.gameObject.SetActive(false),
            bullet => Destroy(bullet.gameObject),
            true,
            10, 100);
    }
    
    private async void Start() => await LoadBulletPrefabAsync();
    
    private Bullet CreateBullet()
    {
        var obj = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.identity);
        return obj.GetComponent<Bullet>();
    }
    
    private async UniTask LoadBulletPrefabAsync()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(bulletAddress);
        bulletPrefab = await handle.ToUniTask();
    }
    
    private void Update()
    {
        UpdateAim();
        TryFire();
    }

    private void UpdateAim()
    {
        if (!TryGetAimTarget(out Vector3 target)) return;

        Vector3 dir = target - turretBase.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rotation = Quaternion.LookRotation(dir);
            turretBase.rotation = Quaternion.Euler(-90f, rotation.eulerAngles.y, 0f);
        }
    }

    private void TryFire()
    {
        if (Input.GetMouseButton(0) && Time.time - _lastFireTime > fireRate)
        {
            _lastFireTime = Time.time;
            FireAsync().Forget();
        }
    }

    private async UniTaskVoid FireAsync()
    {
        if (!TryGetAimTarget(out Vector3 targetPoint)) return;

        Vector3 direction = (targetPoint - muzzlePoint.position).normalized;

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
