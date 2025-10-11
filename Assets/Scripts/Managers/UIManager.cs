using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //Properties
    public static UIManager Instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI SelectedHeroText;


    //Methods
    private void Awake()
    {
        Instance = this;
    }

    public void UpdateSelectedHeroUI(BaseHero hero)
    {
        SelectedHeroText.text = hero != null ? $"{hero.UnitName}" : "N/A";
    }
}
