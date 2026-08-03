using System.Collections;
using UnityEngine;

/// <summary>
/// Ground tile class that represents walkable tiles in the game.
/// </summary>
public class GroundTile : BaseTile
{
    //Properties
    [Header("Colors")]
    [SerializeField] private Color baseColor;
    [SerializeField] private Color offsetColor;

    [Header("Mist")]
    [SerializeField] private float mistFadeSeconds = 2f;
    [SerializeField] private GameObject mistGO;
    [SerializeField] private SpriteRenderer[] mistSprites;
    public bool HasMist { get; private set; } = false;
    public bool AwaitingMistToggle { get; private set; } = false;

    //Methods
    public override bool Walkable => isWalkable && OccupiedUnit == null && HasMist == false && OccupiedChest == null;

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
        if (mistSprites == null || mistSprites.Length <= 0 || mistGO == null) { return; }
        HasMist = isMisty;

        if (!isActiveAndEnabled)
        {
            AwaitingMistToggle = true;
            return;
        }

        mistGO.gameObject.SetActive(isMisty);
        AwaitingMistToggle = false;

        if (isMisty)
        {
            if (OccupiedUnit != null)
            {
                OccupiedUnit.Die();
            }
            else
            {
                StartCoroutine(FadeInMist(mistFadeSeconds));
            }
        }
    }

    private IEnumerator FadeInMist(float duration)
    {
        float timePassed = 0f;
        Color mistColor = mistSprites[0].color;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            mistColor.a = Mathf.Lerp(0f, 1f, timePassed / duration);

            for (int i = 0; i < mistSprites.Length; i++)
            {
                mistSprites[i].color = mistColor;
            }

            yield return null;
        }

        mistColor.a = 1f;
        for (int i = 0; i < mistSprites.Length; i++)
        {
            mistSprites[i].color = mistColor;
        }
    }
}
