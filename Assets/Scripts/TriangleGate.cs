using UnityEngine;

public class TriangleGate : MonoBehaviour
{
    public float openY = 18.1f;
    public float speed = 2f;

    private bool shouldOpen = false;

    void Update()
    {
        if (shouldOpen)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, openY, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }

    public void OpenGate()
    {
        shouldOpen = true;
    }
}
