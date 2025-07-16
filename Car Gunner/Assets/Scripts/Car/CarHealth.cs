using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private TextMeshProUGUI statusText;

    private int _currentHealth;

    private void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            statusText.text = "You Lose";
            Time.timeScale = 0f;
        }
    }
}