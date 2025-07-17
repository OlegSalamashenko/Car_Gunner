using UnityEngine;
using UnityEngine.UI;
using System;

public class CarHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 15;

    private int _currentHealth;

    public event Action OnCarDestroyed;

    private void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        Debug.Log("_currentHealth  "+_currentHealth);
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            OnCarDestroyed?.Invoke();
        }
    }
}