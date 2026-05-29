using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    public static AStarPathfinder Instance { get; private set; }

    [Header("Referencias")]
    public GridManager gridManager;

    [Header("Debug")]
    public bool drawPath = true;
    public Transform debugStart;
    public Transform debugTarget;

    private List<GridNode> lastPath = new List<GridNode>();

    private void Awake()
    {
        Instance = this;

        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

    }

    private void Update()
    {
        if (debugStart != null && debugTarget != null)
        {
            lastPath = FindPath(debugStart.position, debugTarget.position);
        }
    }

    public List<GridNode> FindPath(Vector3 startWorldPosition, Vector3 targetWorldPosition)
    {
        List<GridNode> finalPath = new List<GridNode>();

        if (gridManager == null)
            return finalPath;

        ResetGridCosts();
        GridNode startNode = gridManager.GetNodeFromWorldPosition(startWorldPosition);
        GridNode targetNode = gridManager.GetNodeFromWorldPosition(targetWorldPosition);

        if (startNode == null || targetNode == null)
            return finalPath;

        if (!targetNode.isWalkable)
            return finalPath;

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        List<GridNode> openSet = new List<GridNode>();
        HashSet<GridNode> closedSet = new HashSet<GridNode>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            GridNode currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                bool hasLowerCost = openSet[i].FCost < currentNode.FCost;
                bool hasSameCostButCloser = openSet[i].FCost == currentNode.FCost &&
                                            openSet[i].hCost < currentNode.hCost;

                if (hasLowerCost || hasSameCostButCloser)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                finalPath = RetracePath(startNode, targetNode);
                return finalPath;
            }

            foreach (GridNode neighbour in gridManager.GetNeighbours(currentNode))
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                    continue;

                int newMovementCostToNeighbour =
                    currentNode.gCost +
                    GetDistance(currentNode, neighbour) +
                    neighbour.movementCost;

                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return finalPath;
    }


    private void ResetGridCosts()
    {
        GridNode[,] grid = gridManager.Grid;

        if (grid == null)
            return;

        foreach (GridNode node in grid)
        {
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.parent = null;
        }
    }
    private List<GridNode> RetracePath(GridNode startNode, GridNode endNode)
    {
        List<GridNode> path = new List<GridNode>();

        GridNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;

            if (currentNode == null)
                break;
        }

        path.Reverse();
        return path;
    }

    private int GetDistance(GridNode nodeA, GridNode nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return distanceX + distanceY;
    }

    private void OnDrawGizmos()
    {
        if (!drawPath || lastPath == null)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < lastPath.Count; i++)
        {
            Gizmos.DrawSphere(lastPath[i].worldPosition + Vector3.up * 0.4f, 0.15f);

            if (i < lastPath.Count - 1)
            {
                Gizmos.DrawLine(
                    lastPath[i].worldPosition + Vector3.up * 0.4f,
                    lastPath[i + 1].worldPosition + Vector3.up * 0.4f
                );
            }
        }
    }
}