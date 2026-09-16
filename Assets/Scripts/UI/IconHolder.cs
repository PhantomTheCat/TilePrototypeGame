using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class IconHolder : MonoBehaviour
{
    //Properties
    [Header("Dropdown Field")]
    [SerializeField] private TMP_Dropdown iconDropdown;

    [Header("Icon Color")]
    public DamageType currentDamageType;

    [Header("Sprites")]
    [SerializeField] private Sprite[] physicalSprites;
    [SerializeField] private Sprite[] thunderSprites;
    [SerializeField] private Sprite[] fireSprites;
    [SerializeField] private Sprite[] iceSprites;
    [SerializeField] private Sprite[] poisonSprites;
    [SerializeField] private Sprite[] lightSprites;
    [SerializeField] private Sprite[] darkSprites;
    [SerializeField] private Sprite[] bloodSprites;

    //Methods
    private void Awake()
    {
        if (iconDropdown == null)
        {
            Debug.LogError("Icon Dropdown is not assigned in the inspector.");
            return;
        }
        PopulateDropdown();
    }

    private void PopulateDropdown()
    {
        iconDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        Sprite[] iconSprites = GetSprites();

        foreach (Sprite sprite in iconSprites)
        {
            options.Add(new TMP_Dropdown.OptionData(sprite));
        }

        iconDropdown.AddOptions(options);
        iconDropdown.RefreshShownValue();
    }

    private Sprite[] GetSprites()
    {
        return currentDamageType switch
        {
            DamageType.PHYSICAL => physicalSprites,
            DamageType.THUNDER => thunderSprites,
            DamageType.ICE => iceSprites,
            DamageType.POISON => poisonSprites,
            DamageType.LIGHT => lightSprites,
            DamageType.FIRE => fireSprites,
            DamageType.DARK => darkSprites,
            DamageType.BLOOD => bloodSprites,
            _ => thunderSprites, // Default to purple if no match
        };
    }
}
