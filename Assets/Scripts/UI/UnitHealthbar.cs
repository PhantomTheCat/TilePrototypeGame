using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthbar : MonoBehaviour
{
    //Properties
    [Header("Healthbar GO")]
    [SerializeField] private Slider healthbar;
    [SerializeField] private TextMeshProUGUI healthText;
    private BaseUnit unitAttached;

    //Methods
    public void AssignUnit(BaseUnit unit)
    {
        unitAttached = unit;
        unit.Healthbar = this;
        UpdateBar();
    }

    public void UpdateBar()
    {
        if (unitAttached == null || healthbar == null || healthText == null) return;

        //Subtracting the amount damaged
        float currentHealth = unitAttached.CurrentHealth;
        float currentMaxHealth = unitAttached.MaxHealth;
        float currentMinHealth = 0f;

        currentHealth = Mathf.Clamp(currentHealth, currentMinHealth, currentMaxHealth);
        healthbar.value = currentHealth / currentMaxHealth;
        healthText.text = $"{currentHealth} / {currentMaxHealth}";
    }
}
