using System.Collections.Generic;
using UnityEngine;

public class GuardPathFollower : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;

    [Header("Movimiento")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    public float nodeReachDistance = 0.15f;

    [Header("Pathfinding")]
    public float pathRefreshTime = 0.5f;

    [Header("Debug")]
    public bool drawCurrentPath = true;

    private List<GridNode> currentPath = new List<GridNode>();
    private int currentPathIndex;
    private float pathTimer;

    private void Update()
    {
        if (target == null)
            return;

        pathTimer -= Time.deltaTime;

        if (pathTimer <= 0f)
        {
            RequestNewPath();
            pathTimer = pathRefreshTime;
        }

        FollowPath();
    }

    private void RequestNewPath()
    {
        if (AStarPathfinder.Instance == null)
            return;

        currentPath = AStarPathfinder.Instance.FindPath(transform.position, target.position);
        currentPathIndex = 0;
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        if (currentPathIndex >= currentPath.Count)
            return;

        Vector3 destination = currentPath[currentPathIndex].worldPosition;
        destination.y = transform.position.y;

        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= nodeReachDistance)
        {
            currentPathIndex++;
            return;
        }

        Vector3 movement = direction.normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawCurrentPath || currentPath == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = currentPathIndex; i < currentPath.Count; i++)
        {
            Vector3 pos = currentPath[i].worldPosition + Vector3.up * 0.7f;
            Gizmos.DrawSphere(pos, 0.12f);

            if (i < currentPath.Count - 1)
            {
                Vector3 nextPos = currentPath[i + 1].worldPosition + Vector3.up * 0.7f;
                Gizmos.DrawLine(pos, nextPos);
            }
        }
    }
}