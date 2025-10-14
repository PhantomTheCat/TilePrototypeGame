using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        GridManager.Instance.HighlightHeroTiles();
    }
}
