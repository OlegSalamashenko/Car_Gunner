using UnityEngine;

public class CarMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float swayAmplitude = 1f;
    [SerializeField] private float swayFrequency = 0.5f;

    private float _startX;
    private float _time;
    private bool _isMoving;

    private void Start()
    {
        _startX = transform.position.x;
    }

    private void Update()
    {
        if (!_isMoving) return;

        _time += Time.deltaTime;

        float xOffset = Mathf.Sin(_time * swayFrequency) * swayAmplitude;

        Vector3 newPosition = transform.position;
        newPosition.x = _startX + xOffset;
        newPosition += Vector3.forward * (speed * Time.deltaTime);

        transform.position = newPosition;
    }

    public void StartMoving() => _isMoving = true;

    public void StopMoving() => _isMoving = false;
}