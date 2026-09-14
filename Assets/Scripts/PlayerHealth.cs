using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] float maxHealth;
    [SerializeField] float currentHealth;
    [SerializeField] GameObject deathUI;
    [SerializeField] Slider hpBar;

    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Damage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        hpBar.value = currentHealth / 100;
    }
    public void Heal(float heal)
    {
        currentHealth = Mathf.Clamp(currentHealth + heal, 0f, maxHealth);
        hpBar.value = currentHealth / 100;
    }
    void Die()
    {
       deathUI.SetActive(true);
    }
}
