using UnityEngine;

public enum GridCellType
{
    Walkable,
    Obstacle,
    Dark
}

public class GridNode
{
    public int gridX;
    public int gridY;

    public Vector3 worldPosition;

    public bool isWalkable;
    public bool isDark;

    public int movementCost;

    public int gCost;
    public int hCost;

    public GridNode parent;

    public int FCost
    {
        get { return gCost + hCost; }
    }

    public GridNode(int gridX, int gridY, Vector3 worldPosition, bool isWalkable, bool isDark)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        this.worldPosition = worldPosition;
        this.isWalkable = isWalkable;
        this.isDark = isDark;

        movementCost = isDark ? 5 : 1;
    }
}