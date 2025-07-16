using UnityEngine;
using Zenject;

public class FinishTrigger : MonoBehaviour
{
    public delegate void OnFinishHandler();
    public event OnFinishHandler OnFinish;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnFinish?.Invoke();
        }
    }
}