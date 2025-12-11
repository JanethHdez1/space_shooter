using System;
using UnityEngine;
using UnityEngine.Events;
using SavingSystem;

public class Health : MonoBehaviour, ISavable
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
        
        Debug.Log($"💔 Vida de torreta: {currentHealth}/{maxHealth}");
        
        if (currentHealth == 0f)
        {
            OnDeath.Invoke();
            HandleDeath();
        }
    }
    
    private void HandleDeath()
    {
        Debug.Log("💀 Torreta destruida!");
        
        // Notificar al GameManager para ir a GameOver
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTurretDeath();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChange.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"💚 Torreta curada: {currentHealth}/{maxHealth}");
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChange.Invoke(currentHealth, maxHealth);
    }
    
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;

    public void Save(ref GameData gameData)
    {
        if (gameObject.name.ToLower().Contains("turret"))
        {
            gameData.turretCurrentHealth = currentHealth;
            gameData.turretMaxHealth = maxHealth;
            
            Debug.Log($"💾 Vida de torreta guardada: {currentHealth}/{maxHealth}");
        }
    }
    
    public void Load(ref GameData gameData)
    {
        if (gameObject.name.ToLower().Contains("turret"))
        {
            currentHealth = gameData.turretCurrentHealth;
            maxHealth = gameData.turretMaxHealth;
            
            OnHealthChange.Invoke(currentHealth, maxHealth);
            
            Debug.Log($"📂 Vida de torreta cargada: {currentHealth}/{maxHealth}");
        }
    }
}