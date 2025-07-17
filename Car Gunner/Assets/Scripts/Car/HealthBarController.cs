using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Canvas))]
public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Transform carTransform;

    private CarHealth _carHealth;

    private void Awake() => healthSlider.gameObject.SetActive(false);

    private void Start()
    {
        _carHealth = carTransform.GetComponent<CarHealth>();
        if (_carHealth == null)
        {
            Debug.LogError("HealthBar: CarHealth component not found");
            return;
        }

        _carHealth.OnHealthChanged += UpdateSlider;
        _carHealth.OnCarDestroyed += HideSlider;
    }

    private void OnDestroy()
    {
        if (_carHealth != null)
        {
            _carHealth.OnHealthChanged -= UpdateSlider;
            _carHealth.OnCarDestroyed -= HideSlider;
        }
    }
    private void UpdateSlider(int current, int max)
    {
        bool show = current > 0 && current < max;
        healthSlider.gameObject.SetActive(show);

        if (!show) 
            return;

        healthSlider.maxValue = max;
        healthSlider.value    = current;
    }
    private void HideSlider() => healthSlider.gameObject.SetActive(false);
}