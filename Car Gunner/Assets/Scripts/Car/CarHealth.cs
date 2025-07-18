using UnityEngine;
using System;

public class CarHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 15;
    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action OnCarDestroyed;

    private void Start()
    {
        ResetHealth();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth == 0)
            OnCarDestroyed?.Invoke();
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}