using UnityEngine;

public class DemonGateTrigger : MonoBehaviour
{
    public string playerTag = "Player";
    public float activationDelay = 1.5f;

    private float _timer = 0f;
    private bool _playerInZone = false;
    private bool _transitionTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInZone = true;
            _timer = 0f;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (_playerInZone && !_transitionTriggered)
        {
            _timer += Time.deltaTime;

            if (_timer >= activationDelay)
            {
                _transitionTriggered = true;
                if (LoreTransitionManager.Instance != null)
                {
                    LoreTransitionManager.Instance.StartLoreTransition();
                }
                else
                {
                    Debug.LogError("LoreTransitionManager.Instance is NULL. Make sure it's in the scene and set up.");
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInZone = false;
            _timer = 0f;
        }
    }
}