using UnityEngine;

public class GuardStateLabel : MonoBehaviour
{
    [Header("Referencias")]
    public EnemyBlackboard blackboard;

    [Header("Texto")]
    public TextMesh stateText;
    public Vector3 offset = new Vector3(0f, 1.6f, 0f);
    public float textSize = 0.35f;

    private Camera mainCamera;

    private void Start()
    {
        if (blackboard == null)
            blackboard = GetComponent<EnemyBlackboard>();

        mainCamera = Camera.main;

        if (stateText == null)
            CreateTextObject();
    }

    private void Update()
    {
        if (blackboard == null || stateText == null)
            return;
        if (blackboard.externalAlertDisplayTimer > 0f)
        {
            blackboard.externalAlertDisplayTimer -= Time.deltaTime;
        }

        UpdateText();
        FaceCamera();
    }

    private void CreateTextObject()
    {
        GameObject textObject = new GameObject("State_Label");
        textObject.transform.SetParent(transform);
        textObject.transform.localPosition = offset;

        stateText = textObject.AddComponent<TextMesh>();
        stateText.anchor = TextAnchor.MiddleCenter;
        stateText.alignment = TextAlignment.Center;
        stateText.characterSize = textSize;
        stateText.fontSize = 80;
        stateText.text = "PATRULLA";
        stateText.color = Color.cyan;
    }

    private void UpdateText()
    {
        if (blackboard.externalAlertDisplayTimer > 0f)
        {
            stateText.text = "ALERTA\nRECIBIDA";
            stateText.color = new Color(1f, 0.5f, 0f);
            return;
        }

        switch (blackboard.alertLevel)
        {
            case GuardAlertLevel.Patrol:
                stateText.text = "PATRULLA";
                stateText.color = Color.cyan;
                break;

            case GuardAlertLevel.Suspicious:
                stateText.text = "SOSPECHA";
                stateText.color = Color.yellow;
                break;

            case GuardAlertLevel.Alert:
                stateText.text = "ALERTA";
                stateText.color = Color.red;
                break;

            case GuardAlertLevel.Search:
                stateText.text = "BUSCANDO";
                stateText.color = new Color(1f, 0.5f, 0f);
                break;
        }
    }

    private void FaceCamera()
    {
        if (mainCamera == null)
            return;

        Vector3 directionToCamera = stateText.transform.position - mainCamera.transform.position;

        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            stateText.transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }
}