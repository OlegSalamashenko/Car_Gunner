using UnityEngine;

public class CarMover : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private bool isMoving = false;

    public void StartMoving()
    {
        isMoving = true;
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }
}