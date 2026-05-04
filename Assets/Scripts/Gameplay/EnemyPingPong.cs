using UnityEngine;

public class EnemyPingPong : MonoBehaviour
{
    [SerializeField] private Vector2 moveAxis = Vector2.right;
    [SerializeField] private float distance = 1.5f;
    [SerializeField] private float speed = 1.5f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float t = Mathf.Sin(Time.time * speed);
        transform.position = startPos + (Vector3)(moveAxis.normalized * t * distance);
    }
}
