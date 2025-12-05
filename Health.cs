using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    public UnityEvent<float, float> OnHealthChange;
    public UnityEvent OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0f);
        OnHealthChange.Invoke(currentHealth, maxHealth);
        Debug.Log($"Current Health: {currentHealth}");
        if (currentHealth == 0f) OnDeath.Invoke();
    }
}
