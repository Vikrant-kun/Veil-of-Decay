using UnityEngine;

public class BossIntroTrigger : MonoBehaviour
{
    public BossIntroManager introManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            introManager.StartIntroSequence();
            
            gameObject.SetActive(false);
        }
    }
}