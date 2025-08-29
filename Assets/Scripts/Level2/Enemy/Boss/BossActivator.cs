using UnityEngine;

public class BossActivator : MonoBehaviour
{
    public GameObject archimageGameObject;
    public GameObject bossDoor;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            archimageGameObject.SetActive(true);
            
            if (bossDoor != null)
            {
                bossDoor.SetActive(true);
            }

            Debug.Log("Boss Activated!");
            
            gameObject.SetActive(false);
        }
    }
}