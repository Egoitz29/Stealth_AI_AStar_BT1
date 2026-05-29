using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyBlackboard))]
public class GuardAI_AStarBT : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public LayerMask obstacleLayer;

    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float waypointReachDistance = 0.8f;

    [Header("Movimiento")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    public float nodeReachDistance = 0.25f;

    [Header("Pathfinding")]
    public float pathRefreshTime = 0.5f;

    [Header("Detección")]
    public float normalDetectionRange = 7f;
    public float darkDetectionRange = 3f;
    public float detectionAngle = 90f;
    public float normalDetectionTime = 0.35f;
    public float darkDetectionTime = 1.5f;
    public float suspicionDecreaseSpeed = 1f;

    [Header("Ataque")]
    public float attackRange = 1.4f;
    public float attackCooldown = 1f;

    [Header("Búsqueda")]
    public float searchDuration = 4f;

    [Header("Debug")]
    public bool drawCurrentPath = true;
    public bool drawVision = true;

    private EnemyBlackboard blackboard;
    private PlayerStealth playerStealth;
    private Renderer guardRenderer;
    private BTNode tree;

    private List<GridNode> currentPath = new List<GridNode>();
    private int currentPathIndex;
    [Header("Inicio de patrulla")]
    public int startingPatrolIndex = 0;

    private int currentPatrolIndex;

    private Vector3 currentDestination;
    private bool hasDestination;
    private float pathTimer;
    private float attackTimer;
    private float searchTimer;

    private void Start()
    {
        blackboard = GetComponent<EnemyBlackboard>();
        guardRenderer = GetComponent<Renderer>();

        if (player != null)
        {
            blackboard.player = player;
            playerStealth = player.GetComponent<PlayerStealth>();
        }

        GuardAlertSystem.Instance?.RegisterGuard(this);
        currentPatrolIndex = startingPatrolIndex;
        BuildTree();
    }

    private void OnDestroy()
    {
        GuardAlertSystem.Instance?.UnregisterGuard(this);
    }

    private void Update()
    {
        SensePlayer();

        tree?.Tick();

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;
    }

    private void BuildTree()
    {
        tree = new SelectorNode(
            "Guard Root",

            new SequenceNode(
                "Attack Player",
                new ConditionNode("Can See Player", CanSeePlayer),
                new ActionNode("Chase Or Attack", ChaseOrAttackPlayer)
            ),

            new SequenceNode(
                "Investigate Alert",
                new ConditionNode("Is Alerted", IsAlerted),
                new ActionNode("Investigate Last Position", InvestigateLastKnownPosition)
            ),

            new SequenceNode(
                "Suspicious",
                new ConditionNode("Is Suspicious", IsSuspicious),
                new ActionNode("Look Suspicious", LookSuspicious)
            ),

            new ActionNode("Patrol", Patrol)
        );
    }

    private void SensePlayer()
    {
        blackboard.canSeePlayer = false;

        if (player == null)
            return;

        bool playerIsDark = false;

        if (playerStealth != null)
            playerIsDark = playerStealth.IsInDarkZone;

        blackboard.isPlayerInDarkZone = playerIsDark;

        Vector3 eyePosition = transform.position + Vector3.up * 0.8f;
        Vector3 playerPosition = player.position + Vector3.up * 0.8f;

        Vector3 directionToPlayer = playerPosition - eyePosition;
        float distanceToPlayer = directionToPlayer.magnitude;

        float currentDetectionRange = playerIsDark ? darkDetectionRange : normalDetectionRange;
        float requiredDetectionTime = playerIsDark ? darkDetectionTime : normalDetectionTime;

        if (distanceToPlayer > currentDetectionRange)
        {
            ReduceSuspicion();
            return;
        }

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer.normalized);

        if (angleToPlayer > detectionAngle * 0.5f)
        {
            ReduceSuspicion();
            return;
        }

        bool blockedByObstacle = Physics.Raycast(
            eyePosition,
            directionToPlayer.normalized,
            distanceToPlayer,
            obstacleLayer
        );

        if (blockedByObstacle)
        {
            ReduceSuspicion();
            return;
        }

        blackboard.suspicionAmount += Time.deltaTime;
        blackboard.SetSuspicious(player.position);

        if (blackboard.suspicionAmount >= requiredDetectionTime)
        {
            blackboard.SetPlayerSeen(player.position);
        }
    }

    private void ReduceSuspicion()
    {
        if (blackboard.alertLevel == GuardAlertLevel.Alert)
            return;

        blackboard.suspicionAmount -= suspicionDecreaseSpeed * Time.deltaTime;
        blackboard.suspicionAmount = Mathf.Max(blackboard.suspicionAmount, 0f);

        if (blackboard.suspicionAmount <= 0f &&
            blackboard.alertLevel == GuardAlertLevel.Suspicious)
        {
            blackboard.alertLevel = GuardAlertLevel.Patrol;
            blackboard.hasLastKnownPlayerPosition = false;
        }
    }

    private bool CanSeePlayer()
    {
        return blackboard.canSeePlayer;
    }

    private bool IsAlerted()
    {
        return blackboard.alertLevel == GuardAlertLevel.Alert ||
               blackboard.alertLevel == GuardAlertLevel.Search;
    }

    private bool IsSuspicious()
    {
        return blackboard.alertLevel == GuardAlertLevel.Suspicious;
    }

    private NodeStatus ChaseOrAttackPlayer()
    {
        SetColor(Color.red);

        if (player == null)
            return NodeStatus.Failure;

        blackboard.SetPlayerSeen(player.position);
        GuardAlertSystem.Instance?.AlertAllGuards(this, player.position);

        float distanceToPlayer = Vector3.Distance(
            GetFlatPosition(transform.position),
            GetFlatPosition(player.position)
        );

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
            return NodeStatus.Running;
        }

        return MoveTo(player.position, attackRange);
    }

    private void AttackPlayer()
    {
        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;
        Debug.Log($"{gameObject.name} ataca al jugador.");
    }

    private NodeStatus InvestigateLastKnownPosition()
    {
        SetColor(new Color(1f, 0.5f, 0f));

        if (!blackboard.hasLastKnownPlayerPosition)
            return NodeStatus.Failure;

        NodeStatus moveStatus = MoveTo(blackboard.lastKnownPlayerPosition, 0.7f);

        if (moveStatus == NodeStatus.Running)
            return NodeStatus.Running;

        blackboard.alertLevel = GuardAlertLevel.Search;
        searchTimer += Time.deltaTime;

        transform.Rotate(Vector3.up, 80f * Time.deltaTime);

        if (searchTimer >= searchDuration)
        {
            searchTimer = 0f;
            hasDestination = false;
            currentPath.Clear();
            blackboard.ReturnToPatrol();
            return NodeStatus.Success;
        }

        return NodeStatus.Running;
    }

    private NodeStatus LookSuspicious()
    {
        SetColor(Color.yellow);

        if (blackboard.hasLastKnownPlayerPosition)
        {
            Vector3 direction = blackboard.lastKnownPlayerPosition - transform.position;
            direction.y = 0f;

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

        return NodeStatus.Running;
    }

    private NodeStatus Patrol()
    {
        SetColor(Color.cyan);

        if (patrolPoints == null || patrolPoints.Length == 0)
            return NodeStatus.Failure;

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        if (targetPoint == null)
            return NodeStatus.Failure;

        NodeStatus moveStatus = MoveTo(targetPoint.position, waypointReachDistance);

        if (moveStatus == NodeStatus.Success)
        {
            currentPatrolIndex++;

            if (currentPatrolIndex >= patrolPoints.Length)
                currentPatrolIndex = 0;

            hasDestination = false;
        }

        return NodeStatus.Running;
    }

    private NodeStatus MoveTo(Vector3 destination, float reachDistance)
    {
        float distanceToDestination = Vector3.Distance(
            GetFlatPosition(transform.position),
            GetFlatPosition(destination)
        );

        if (distanceToDestination <= reachDistance)
        {
            return NodeStatus.Success;
        }

        bool destinationChanged =
            !hasDestination ||
            Vector3.Distance(GetFlatPosition(currentDestination), GetFlatPosition(destination)) > 0.25f;

        if (destinationChanged)
        {
            currentDestination = destination;
            hasDestination = true;
            RequestNewPath(destination);
            pathTimer = pathRefreshTime;
        }

        pathTimer -= Time.deltaTime;

        if (pathTimer <= 0f)
        {
            RequestNewPath(destination);
            pathTimer = pathRefreshTime;
        }

        FollowPath();

        bool pathFinished =
            currentPath != null &&
            currentPath.Count > 0 &&
            currentPathIndex >= currentPath.Count;

        if (pathFinished)
        {
            return NodeStatus.Success;
        }

        return NodeStatus.Running;
    }

    private void RequestNewPath(Vector3 destination)
    {
        if (AStarPathfinder.Instance == null)
            return;

        currentPath = AStarPathfinder.Instance.FindPath(transform.position, destination);
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

    public void ReceiveAlert(Vector3 playerPosition)
    {
        if (blackboard == null)
            blackboard = GetComponent<EnemyBlackboard>();

        blackboard.ReceiveExternalAlert(playerPosition);

        hasDestination = false;
        currentPath.Clear();
        searchTimer = 0f;
    }

    private Vector3 GetFlatPosition(Vector3 position)
    {
        return new Vector3(position.x, 0f, position.z);
    }

    private void SetColor(Color color)
    {
        if (guardRenderer != null)
            guardRenderer.material.color = color;
    }

    private void OnDrawGizmos()
    {
        DrawPathGizmos();
        DrawVisionGizmos();
    }

    private void DrawPathGizmos()
    {
        if (!drawCurrentPath || currentPath == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = currentPathIndex; i < currentPath.Count; i++)
        {
            Vector3 pos = currentPath[i].worldPosition + Vector3.up * 0.8f;
            Gizmos.DrawSphere(pos, 0.12f);

            if (i < currentPath.Count - 1)
            {
                Vector3 nextPos = currentPath[i + 1].worldPosition + Vector3.up * 0.8f;
                Gizmos.DrawLine(pos, nextPos);
            }
        }
    }

    private void DrawVisionGizmos()
    {
        if (!drawVision)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, normalDetectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, darkDetectionRange);
    }
}