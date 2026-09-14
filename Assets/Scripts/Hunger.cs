using UnityEngine;
using UnityEngine.UI;

public class Hunger : MonoBehaviour
{
    [Header("Hunger")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float currentHunger = 100f;
    [SerializeField] private float hungerDecreasePerSecond = 1f;
    [SerializeField] private PlayerHealth health;

    [Header("UI")]
    [SerializeField] private Slider hungerSlider;

    void Start()
    {
        hungerSlider.maxValue = maxHunger;
        hungerSlider.value = currentHunger;
    }

    void Update()
    {
        currentHunger -= hungerDecreasePerSecond * Time.deltaTime;

        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        if(currentHunger <= 0)
        {
            health.Damage(0.5f * Time.deltaTime);
        }

        hungerSlider.value = currentHunger;
    }

    public void IncreaseHunger(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        hungerSlider.value = currentHunger;
    }
}
