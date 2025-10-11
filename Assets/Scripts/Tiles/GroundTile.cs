using UnityEngine;

/// <summary>
/// Ground tile class that represents walkable tiles in the game.
/// </summary>
public class GroundTile : BaseTile
{
    //Properties
    [SerializeField] private Color baseColor;
    [SerializeField] private Color offsetColor;

    //Methods
    public override void Activate(ICoords coords)
    {
        base.Activate(coords);

        if (spriteRenderer == null) { spriteRenderer = GetComponent<SpriteRenderer>(); }

        //Setting the color based on if the tile is offset or not
        if ((coords.Pos.x + coords.Pos.y) % 2 == 1)
        {
            spriteRenderer.color = baseColor;
        }
        else
        {
            spriteRenderer.color = offsetColor;
        }
    }
}
