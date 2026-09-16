using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static BaseAction;

public class MagicCreationScreen : MonoBehaviour
{
    //Properties
    [Header("References")]
    [SerializeField] private IconHolder iconHolder;
    [SerializeField] private InputField nameInputField;

    //private string magicName;
    //private Sprite magicIcon;
    //private DamageType damageType;
    //private SpellType spellType;
    //private AffectableStat scalingAttribute;
    //private int actionCost;
    //private int manaCost;
    //private int valueAmount;
    //private AreaType areaShape;
    //private RangeType typeOfRange;
    //private int range = 1;
    //private int horizontalArea = 1;
    //private int verticalArea = 1;
    //private int maxTargets = 1;
    //private bool CanTargetUser = false;



    //Methods
    public void CreateSpell()
    {

    }

}

public enum SpellType
{
    ATTACK,
    HEAL,
    SUMMON,
    BUFF,
    DEBUFF
}
