using UnityEngine;

public class HorizontalChaser : MonoBehaviour
{
    public float followSpeed = 8f;

    private Transform playerTransform;

    void Start()
    {
        if (PlayerMovement.Instance != null)
        {
            playerTransform = PlayerMovement.Instance.transform;
        }
        else
        {
            Debug.LogError("HorizontalChaser could not find the Player!");
            this.enabled = false; 
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            Vector3 currentPosition = transform.position;
            
            float targetX = playerTransform.position.x;

            float newX = Mathf.Lerp(currentPosition.x, targetX, followSpeed * Time.deltaTime);

            transform.position = new Vector3(newX, currentPosition.y, currentPosition.z);
        }
    }
}