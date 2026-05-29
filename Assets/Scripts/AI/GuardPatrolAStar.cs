using System.Collections.Generic;
using UnityEngine;

public class GuardPatrolAStar : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float waypointReachDistance = 0.35f;

    [Header("Movimiento")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    public float nodeReachDistance = 0.2f;

    [Header("Pathfinding")]
    public float pathRefreshTime = 0.5f;

    [Header("Debug")]
    public bool drawCurrentPath = true;

    private List<GridNode> currentPath = new List<GridNode>();
    private int currentPathIndex;
    private int currentPatrolIndex;
    private float pathTimer;

    private void Update()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform currentTarget = patrolPoints[currentPatrolIndex];

        if (currentTarget == null)
            return;

        pathTimer -= Time.deltaTime;

        if (pathTimer <= 0f)
        {
            RequestNewPath(currentTarget.position);
            pathTimer = pathRefreshTime;
        }

        FollowPath();

        float distanceToPatrolPoint = Vector3.Distance(
            GetFlatPosition(transform.position),
            GetFlatPosition(currentTarget.position)
        );

        bool reachedPatrolPoint = distanceToPatrolPoint <= waypointReachDistance;
        bool finishedPath = currentPath != null && currentPath.Count > 0 && currentPathIndex >= currentPath.Count;

        if (reachedPatrolPoint || finishedPath)
        {
            GoToNextPatrolPoint();
        }
    }

    private void RequestNewPath(Vector3 targetPosition)
    {
        if (AStarPathfinder.Instance == null)
            return;

        currentPath = AStarPathfinder.Instance.FindPath(transform.position, targetPosition);
        currentPathIndex = 0;
    }

    private void GoToNextPatrolPoint()
    {
        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
            currentPatrolIndex = 0;

        RequestNewPath(patrolPoints[currentPatrolIndex].position);
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

    private Vector3 GetFlatPosition(Vector3 position)
    {
        return new Vector3(position.x, 0f, position.z);
    }

    private void OnDrawGizmos()
    {
        DrawPathGizmos();
        DrawPatrolGizmos();
    }

    private void DrawPathGizmos()
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

    private void DrawPatrolGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            Gizmos.DrawSphere(patrolPoints[i].position + Vector3.up * 0.3f, 0.25f);

            int nextIndex = i + 1;

            if (nextIndex >= patrolPoints.Length)
                nextIndex = 0;

            if (patrolPoints[nextIndex] != null)
            {
                Gizmos.DrawLine(
                    patrolPoints[i].position + Vector3.up * 0.3f,
                    patrolPoints[nextIndex].position + Vector3.up * 0.3f
                );
            }
        }
    }
}