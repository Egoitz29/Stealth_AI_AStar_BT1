using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Tamaño del grid")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;

    [Header("Detección de casillas")]
    public LayerMask obstacleLayer;
    public LayerMask darkZoneLayer;

    [Header("Visualización")]
    public bool drawGrid = true;
    public float gizmoHeight = 0.05f;

    private GridNode[,] grid;

    public GridNode[,] Grid
    {
        get { return grid; }
    }

    private void Awake()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        grid = new GridNode[gridWidth, gridHeight];

        Vector3 bottomLeft = transform.position
            - Vector3.right * gridWidth * cellSize / 2f
            - Vector3.forward * gridHeight * cellSize / 2f;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPoint = bottomLeft
                    + Vector3.right * (x * cellSize + cellSize / 2f)
                    + Vector3.forward * (y * cellSize + cellSize / 2f);

                bool hasObstacle = Physics.CheckBox(
                    worldPoint,
                    new Vector3(cellSize * 0.45f, 0.5f, cellSize * 0.45f),
                    Quaternion.identity,
                    obstacleLayer
                );

                bool isDark = Physics.CheckBox(
                    worldPoint,
                    new Vector3(cellSize * 0.45f, 0.5f, cellSize * 0.45f),
                    Quaternion.identity,
                    darkZoneLayer
                );

                bool isWalkable = !hasObstacle;

                grid[x, y] = new GridNode(x, y, worldPoint, isWalkable, isDark);
            }
        }
    }

    public GridNode GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - transform.position;

        float percentX = (localPosition.x + gridWidth * cellSize / 2f) / (gridWidth * cellSize);
        float percentY = (localPosition.z + gridHeight * cellSize / 2f) / (gridHeight * cellSize);

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridWidth - 1) * percentX);
        int y = Mathf.RoundToInt((gridHeight - 1) * percentY);

        return grid[x, y];
    }

    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();

        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                if (offsetX == 0 && offsetY == 0)
                    continue;

                if (Mathf.Abs(offsetX) == 1 && Mathf.Abs(offsetY) == 1)
                    continue;

                int checkX = node.gridX + offsetX;
                int checkY = node.gridY + offsetY;

                if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    private void OnDrawGizmos()
    {
        if (!drawGrid)
            return;

        CreateGrid();

        if (grid == null)
            return;

        foreach (GridNode node in grid)
        {
            if (!node.isWalkable)
            {
                Gizmos.color = Color.red;
            }
            else if (node.isDark)
            {
                Gizmos.color = new Color(0.2f, 0.1f, 0.8f, 0.6f);
            }
            else
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            }

            Vector3 drawPosition = node.worldPosition + Vector3.up * gizmoHeight;
            Gizmos.DrawCube(drawPosition, Vector3.one * (cellSize * 0.9f));
        }
    }
}