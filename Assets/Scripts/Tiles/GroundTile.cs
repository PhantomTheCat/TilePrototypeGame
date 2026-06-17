using UnityEngine;

/// <summary>
/// Ground tile class that represents walkable tiles in the game.
/// </summary>
public class GroundTile : BaseTile
{
    //Properties
    public override bool Walkable => isWalkable && OccupiedUnit == null && HasMist == false;

    [Header("Colors")]
    [SerializeField] private Color baseColor;
    [SerializeField] private Color offsetColor;

    [Header("Mist")]
    [SerializeField] private GameObject mistGO;
    public bool HasMist { get; private set; } = false;

    //Methods
    public override void Activate(ICoords coords)
    {
        base.Activate(coords);

        if (spriteRenderer == null) { spriteRenderer = GetComponent<SpriteRenderer>(); }

        //Setting the color based on if the tile is offset or not
        int isOffset = Mathf.Abs((int)coords.Pos.x + (int)coords.Pos.y) % 2;
        if (isOffset == 1)
        {
            spriteRenderer.color = baseColor;
        }
        else
        {
            spriteRenderer.color = offsetColor;
        }
    }

    public void ToggleMist(bool isMisty)
    {
        if (mistGO == null) { return; }

        HasMist = isMisty;
        mistGO.SetActive(isMisty);

        if (OccupiedUnit != null)
        {
            OccupiedUnit.Die();
        }
    }
}
