using UnityEngine;

public class CarMover : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float swayAmplitude = 0.5f; // амплитуда X-виляния
    [SerializeField] private float swayFrequency = 0.5f; // частота виляния (чем меньше — тем плавнее)

    private float _startX;
    private float _time;

    private bool _moving;

    private void Start()
    {
        _startX = transform.position.x;
    }

    private void Update()
    {
        if (!_moving) return;

        _time += Time.deltaTime;

        float xOffset = Mathf.Sin(_time * swayFrequency) * swayAmplitude;

        Vector3 pos = transform.position;
        pos.x = _startX + xOffset;
        pos += Vector3.forward * (speed * Time.deltaTime);

        transform.position = pos;
    }

    public void StartMoving()
    {
        _moving = true;
    }
}