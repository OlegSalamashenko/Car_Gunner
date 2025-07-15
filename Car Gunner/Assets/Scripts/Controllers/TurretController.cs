using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

public class TurretController : MonoBehaviour
{
    [SerializeField] private Transform turretBase;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private float fireRate = 0.25f;

    private float _lastFireTime;

    private void Update()
    {
        Aim();
        HandleFire();
    }

    private void Aim()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask))
        {
            Vector3 target = hit.point;
            target.y = turretBase.position.y;

            Vector3 dir = target - turretBase.position;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                turretBase.rotation = Quaternion.Euler(-90f, rot.eulerAngles.y, 0);
            }
        }
    }

    private void HandleFire()
    {
        if (Input.GetMouseButton(0) && Time.time - _lastFireTime > fireRate)
        {
            _lastFireTime = Time.time;
            FireAsync().Forget();
        }
    }

    private async UniTaskVoid FireAsync()
    {
        Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        await UniTask.Yield();
    }
}