using UnityEngine;

public class PlayerStealth : MonoBehaviour
{
    [Header("Referencias")]
    public GridManager gridManager;

    public bool IsInDarkZone { get; private set; }

    private void Start()
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();
    }

    private void Update()
    {
        UpdateDarkZoneState();
    }

    private void UpdateDarkZoneState()
    {
        if (gridManager == null)
        {
            IsInDarkZone = false;
            return;
        }

        GridNode currentNode = gridManager.GetNodeFromWorldPosition(transform.position);

        if (currentNode == null)
        {
            IsInDarkZone = false;
            return;
        }

        IsInDarkZone = currentNode.isDark;
    }
}