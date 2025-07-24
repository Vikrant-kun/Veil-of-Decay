using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float offsetZ = -10f; 
    public float offsetY = 2f; 
    public float smoothSpeed = 7f; 

    void LateUpdate() 
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + new Vector3(0, offsetY, offsetZ);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log("CameraFollow: Target set to " + newTarget.name);
    }
}