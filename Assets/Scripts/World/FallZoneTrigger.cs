using UnityEngine;

public class FallZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("FallZoneTrigger: OnTriggerEnter2D fired! Collided with: " + other.gameObject.name + " Tag: " + other.tag); // NEW LOG

        if (other.CompareTag("Player"))
        {
            Debug.Log("FallZoneTrigger: Player fell into Death Zone! Attempting to trigger death scene."); // NEW LOG

            DeathHandler deathHandler = Object.FindAnyObjectByType<DeathHandler>();
            if (deathHandler != null)
            {
                deathHandler.TriggerDeathScene();
                Debug.Log("FallZoneTrigger: DeathHandler.TriggerDeathScene() called successfully."); // NEW LOG
            }
            else
            {
                Debug.LogError("FallZoneTrigger: DeathHandler not found in the scene! Cannot trigger death scene.", this);
            }
        }
    }
}