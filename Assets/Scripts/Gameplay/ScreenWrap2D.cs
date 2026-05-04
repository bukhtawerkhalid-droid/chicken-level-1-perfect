using UnityEngine;

public class ScreenWrap2D : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float horizontalPadding = 0.02f;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 viewport = targetCamera.WorldToViewportPoint(transform.position);
        if (viewport.x < -horizontalPadding)
        {
            viewport.x = 1f + horizontalPadding;
            transform.position = targetCamera.ViewportToWorldPoint(viewport);
        }
        else if (viewport.x > 1f + horizontalPadding)
        {
            viewport.x = -horizontalPadding;
            transform.position = targetCamera.ViewportToWorldPoint(viewport);
        }
    }
}
