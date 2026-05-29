using System.Collections.Generic;
using UnityEngine;

public class GuardAlertSystem : MonoBehaviour
{
    public static GuardAlertSystem Instance { get; private set; }

    private readonly List<GuardAI_AStarBT> guards = new List<GuardAI_AStarBT>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterGuard(GuardAI_AStarBT guard)
    {
        if (guard == null)
            return;

        if (!guards.Contains(guard))
            guards.Add(guard);
    }

    public void UnregisterGuard(GuardAI_AStarBT guard)
    {
        if (guard == null)
            return;

        if (guards.Contains(guard))
            guards.Remove(guard);
    }

    public void AlertAllGuards(GuardAI_AStarBT sourceGuard, Vector3 playerPosition)
    {
        foreach (GuardAI_AStarBT guard in guards)
        {
            if (guard == null)
                continue;

            if (guard == sourceGuard)
                continue;

            guard.ReceiveAlert(playerPosition);
        }
    }
}