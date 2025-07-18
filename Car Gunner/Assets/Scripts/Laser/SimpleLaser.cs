using UnityEngine;

public class SimpleLaser : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float laserLength = 100f;

    private void Update()
    {
        Vector3 startPos = firePoint.position;
        Vector3 endPos = startPos + firePoint.forward * laserLength;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }
}