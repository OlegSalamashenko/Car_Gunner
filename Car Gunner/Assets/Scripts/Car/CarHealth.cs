using UnityEngine;
 using UnityEngine.UI;
 using System;
 
 public class CarHealth : MonoBehaviour
 {
     [SerializeField] private int maxHealth = 15;
     private int _currentHealth;
     public event Action<int,int> OnHealthChanged;
     public event Action OnCarDestroyed;
 
     private void Start()
     {
         _currentHealth = maxHealth;
         OnHealthChanged?.Invoke(_currentHealth, maxHealth);
     }
 
     public void TakeDamage(int amount)
     {
         _currentHealth = Mathf.Clamp(_currentHealth - amount, 0, maxHealth);
         OnHealthChanged?.Invoke(_currentHealth, maxHealth);
 
         if (_currentHealth == 0)
             OnCarDestroyed?.Invoke();
     }
 }