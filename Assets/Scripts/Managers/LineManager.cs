using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineManager : MonoBehaviour
{
    //Properties
    public static LineManager Instance;
    private LineRenderer lineRenderer;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float zOffset = 0.1f;
    [SerializeField] private float offsetFromStart = 0.5f;


    //Methods
    private void Awake()
    {
        Instance = this;
        lineRenderer = GetComponent<LineRenderer>();

        // Setting up the line renderer
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }

    /// <summary>
    /// By passing in a list of tile in-order, this will draw a line through them
    /// </summary>
    /// <param name="tiles"></param>
    public void DrawLine(List<BaseTile> tiles, BaseTile startTile, Vector2 startDir)
    {
        //Setting the number of positions to the number of tiles in the path
        lineRenderer.positionCount = tiles.Count + 1;

        //Getting the start position with an offset so it doesn't start directly at the center of the tile
        Vector3 startPosition = startTile.transform.position + new Vector3(startDir.x * offsetFromStart, startDir.y * offsetFromStart, -zOffset);
        lineRenderer.SetPosition(0, startPosition);


        for (int i = 0; i < tiles.Count; i++)
        {
            Vector3 tilePosition = tiles[i].transform.position;
            tilePosition.z -= zOffset;
            lineRenderer.SetPosition(i + 1, tilePosition);

            //At last position, going to add the arrowhead
            //if (i == tiles.Count - 1 && tiles.Count > 1)
            //{
            //    // Getting direction and calculating arrowhead points
            //    Vector3 direction = (tiles[i].transform.position - tiles[i - 1].transform.position).normalized;
            //    float arrowHeadLength = 0.25f;
            //    float arrowHeadAngle = 20.0f;
            //    Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            //    Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

            //    // Adding two more points for the arrowhead (Using the line renderer to make this)
            //    lineRenderer.positionCount += 2;
            //    lineRenderer.SetPosition(lineRenderer.positionCount - 2, tiles[i].transform.position + right * arrowHeadLength);
            //    lineRenderer.SetPosition(lineRenderer.positionCount - 1, tiles[i].transform.position + left * arrowHeadLength);
            //}
        }
    }

    public void ClearLine()
    {
        lineRenderer.positionCount = 0;
    }
}
