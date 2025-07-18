using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Transform _carTransform;

    private CarHealth _carHealth;

    private void Awake()
    {
        _healthSlider.gameObject.SetActive(false);
    }

    private void Start()
    {
        _carHealth = _carTransform.GetComponent<CarHealth>();

        _carHealth.OnHealthChanged += UpdateSlider;
        _carHealth.OnCarDestroyed += HideSlider;
    }

    private void OnDestroy()
    {
        if (_carHealth == null) return;

        _carHealth.OnHealthChanged -= UpdateSlider;
        _carHealth.OnCarDestroyed -= HideSlider;
    }

    private void UpdateSlider(int current, int max)
    {
        if (current <= 0 || current >= max)
        {
            HideSlider();
            return;
        }

        _healthSlider.maxValue = max;
        _healthSlider.value = current;

        if (!_healthSlider.gameObject.activeSelf)
        {
            _healthSlider.gameObject.SetActive(true);
        }
    }

    private void HideSlider()
    {
        _healthSlider.gameObject.SetActive(false);
    }
}