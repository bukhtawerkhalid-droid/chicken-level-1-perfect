using UnityEngine;

public class WorldAnchor : MonoBehaviour
{
    public enum AnchorPoint
    {
        TopLeft,
        TopRight,
        TopCenter,
        BottomLeft,
        BottomRight,
        Center
    }

    [Header("Anchor Settings")]
    public AnchorPoint anchorPoint = AnchorPoint.TopLeft;
    public Vector3 offset;
    public bool updateEveryFrame = true;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        UpdatePosition();
    }

    private void LateUpdate()
    {
        if (updateEveryFrame)
        {
            UpdatePosition();
        }
    }

    public void UpdatePosition()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        float camHeight = mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;
        Vector3 camPos = mainCam.transform.position;

        Vector3 targetPos = camPos;
        targetPos.z = transform.position.z; // Keep original Z depth

        switch (anchorPoint)
        {
            case AnchorPoint.TopLeft:
                targetPos.x -= camWidth;
                targetPos.y += camHeight;
                break;
            case AnchorPoint.TopRight:
                targetPos.x += camWidth;
                targetPos.y += camHeight;
                break;
            case AnchorPoint.TopCenter:
                targetPos.y += camHeight;
                break;
            case AnchorPoint.BottomLeft:
                targetPos.x -= camWidth;
                targetPos.y -= camHeight;
                break;
            case AnchorPoint.BottomRight:
                targetPos.x += camWidth;
                targetPos.y -= camHeight;
                break;
        }

        transform.position = targetPos + offset;
    }
}
