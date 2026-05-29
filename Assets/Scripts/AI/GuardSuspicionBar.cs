using UnityEngine;

public class GuardSuspicionBar : MonoBehaviour
{
    [Header("Referencias")]
    public EnemyBlackboard blackboard;
    public GuardAI_AStarBT guardAI;

    [Header("Visual")]
    public Vector3 offset = new Vector3(0f, 2.1f, 0f);
    public Vector3 barSize = new Vector3(1.2f, 0.12f, 0.08f);

    private GameObject backgroundBar;
    private GameObject fillBar;

    private Renderer backgroundRenderer;
    private Renderer fillRenderer;

    private Camera mainCamera;

    private void Start()
    {
        if (blackboard == null)
            blackboard = GetComponent<EnemyBlackboard>();

        if (guardAI == null)
            guardAI = GetComponent<GuardAI_AStarBT>();

        mainCamera = Camera.main;

        CreateBar();
    }

    private void Update()
    {
        if (blackboard == null || guardAI == null)
            return;

        UpdateBar();
        FaceCamera();
    }

    private void CreateBar()
    {
        backgroundBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backgroundBar.name = "Suspicion_Background";
        backgroundBar.transform.SetParent(transform);
        backgroundBar.transform.localPosition = offset;
        backgroundBar.transform.localScale = barSize;

        fillBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fillBar.name = "Suspicion_Fill";
        fillBar.transform.SetParent(transform);
        fillBar.transform.localPosition = offset + new Vector3(-barSize.x * 0.5f, 0.01f, 0f);
        fillBar.transform.localScale = new Vector3(0f, barSize.y, barSize.z * 1.1f);

        backgroundRenderer = backgroundBar.GetComponent<Renderer>();
        fillRenderer = fillBar.GetComponent<Renderer>();

        backgroundRenderer.material.color = Color.black;
        fillRenderer.material.color = Color.yellow;

        Destroy(backgroundBar.GetComponent<Collider>());
        Destroy(fillBar.GetComponent<Collider>());
    }

    private void UpdateBar()
    {
        float maxSuspicionTime = blackboard.isPlayerInDarkZone
            ? guardAI.darkDetectionTime
            : guardAI.normalDetectionTime;

        float fillAmount = Mathf.Clamp01(blackboard.suspicionAmount / maxSuspicionTime);

        if (blackboard.alertLevel == GuardAlertLevel.Alert)
            fillAmount = 1f;

        bool shouldShow =
            fillAmount > 0.01f ||
            blackboard.alertLevel == GuardAlertLevel.Suspicious ||
            blackboard.alertLevel == GuardAlertLevel.Alert;

        backgroundBar.SetActive(shouldShow);
        fillBar.SetActive(shouldShow);

        Vector3 newScale = barSize;
        newScale.x = barSize.x * fillAmount;
        fillBar.transform.localScale = newScale;

        float localX = -barSize.x * 0.5f + newScale.x * 0.5f;
        fillBar.transform.localPosition = offset + new Vector3(localX, 0.01f, 0f);

        if (blackboard.alertLevel == GuardAlertLevel.Alert)
        {
            fillRenderer.material.color = Color.red;
        }
        else if (blackboard.isPlayerInDarkZone)
        {
            fillRenderer.material.color = new Color(0.4f, 0.2f, 1f);
        }
        else
        {
            fillRenderer.material.color = Color.yellow;
        }
    }

    private void FaceCamera()
    {
        if (mainCamera == null)
            return;

        Vector3 directionToCamera = backgroundBar.transform.position - mainCamera.transform.position;

        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);

            backgroundBar.transform.rotation = targetRotation;
            fillBar.transform.rotation = targetRotation;
        }
    }
}