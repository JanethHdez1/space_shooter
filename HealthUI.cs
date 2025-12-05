using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private string label="Current Health";
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Image healthBar;

    public void UpdateHealth(float health, float maxHealth)
    {
        textComponent.text = $"{label}: {health}";
        var normalizedHealth = health / maxHealth;
        healthBar.fillAmount = normalizedHealth;
    } 
}
