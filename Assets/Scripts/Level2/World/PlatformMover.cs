using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public Transform endPoint;
    public float speed = 3f;

    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        targetPosition = endPoint.position;
    }

    void FixedUpdate()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget > 0.1f)
        {
            Vector2 direction = (targetPosition - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            if (targetPosition == startPosition)
            {
                targetPosition = endPoint.position;
            }
            else
            {
                targetPosition = startPosition;
            }
        }
    }
}