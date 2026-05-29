using UnityEngine;

public enum GuardAlertLevel
{
    Patrol,
    Suspicious,
    Alert,
    Search
}

public class EnemyBlackboard : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Percepción")]
    public bool canSeePlayer;
    public bool isPlayerInDarkZone;

    [Header("Memoria")]
    public bool hasLastKnownPlayerPosition;
    public Vector3 lastKnownPlayerPosition;

    [Header("Estado")]
    public GuardAlertLevel alertLevel = GuardAlertLevel.Patrol;
    public float suspicionAmount;
    public bool wasAlertedByOtherGuard;
    public float externalAlertDisplayTimer;

    public void SetPlayerSeen(Vector3 playerPosition)
    {
        canSeePlayer = true;
        hasLastKnownPlayerPosition = true;
        lastKnownPlayerPosition = playerPosition;
        alertLevel = GuardAlertLevel.Alert;
        wasAlertedByOtherGuard = false;
    }

    public void SetSuspicious(Vector3 suspiciousPosition)
    {
        if (alertLevel == GuardAlertLevel.Patrol)
            alertLevel = GuardAlertLevel.Suspicious;

        hasLastKnownPlayerPosition = true;
        lastKnownPlayerPosition = suspiciousPosition;
    }
    public void ReceiveExternalAlert(Vector3 alertPosition)
    {
        hasLastKnownPlayerPosition = true;
        lastKnownPlayerPosition = alertPosition;
        alertLevel = GuardAlertLevel.Alert;
        suspicionAmount = 999f;

        wasAlertedByOtherGuard = true;
        externalAlertDisplayTimer = 3f;
    }

    public void ReturnToPatrol()
    {
        canSeePlayer = false;
        hasLastKnownPlayerPosition = false;
        alertLevel = GuardAlertLevel.Patrol;
        suspicionAmount = 0f;

        wasAlertedByOtherGuard = false;
        externalAlertDisplayTimer = 0f;
    }
}