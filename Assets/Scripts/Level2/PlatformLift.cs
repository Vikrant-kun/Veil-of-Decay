using UnityEngine;
using System.Collections;

public class PlatformLift : MonoBehaviour
{
    [Header("Lift Settings")]
    [SerializeField] private float moveSpeed = 2f; // Speed at which the lift moves upwards
    [SerializeField] private Transform targetPosition; // The empty GameObject marking the lift's destination
    [SerializeField] private float delayBeforeMove = 1f; // Time player needs to stand on it before it moves
    [SerializeField] private bool oneWayLift = true; // If true, the lift only goes up once per scene load

    private Vector3 initialPosition; // Stores the lift's starting position
    private bool hasStartedMoving = false;
    private bool playerIsCurrentlyOnPlatform = false; // Tracks if player is physically on the platform
    private float timePlayerSteppedOn = 0f; // Time when player first stepped on
    private Rigidbody2D rb;

    void Awake()
    {
        initialPosition = transform.position; // Save the starting position
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlatformLift: Missing Rigidbody2D component. Please add one to the lift GameObject and set its Body Type to Kinematic.", this);
        }
    }

    void Update()
    {
        // If player is on the platform and it hasn't started moving yet
        if (playerIsCurrentlyOnPlatform && !hasStartedMoving)
        {
            // Check if the delay has passed since the player stepped on
            if (Time.time - timePlayerSteppedOn >= delayBeforeMove)
            {
                StartLift();
            }
        }
    }

    // Called when another collider starts touching this one
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the Player and the lift hasn't started moving yet
        if (collision.collider.CompareTag("Player") && !hasStartedMoving)
        {
            playerIsCurrentlyOnPlatform = true;
            timePlayerSteppedOn = Time.time; // Record the time the player stepped on

            // Parent the player to the platform so they move with it
            collision.collider.transform.SetParent(transform);
        }
    }

    // Called when another collider stops touching this one
    private void OnCollisionExit2D(Collision2D collision)
    {
        // If the player leaves the platform
        if (collision.collider.CompareTag("Player"))
        {
            // If the lift hasn't started moving, reset the state
            if (!hasStartedMoving)
            {
                playerIsCurrentlyOnPlatform = false;
            }
            // Unparent the player
            collision.collider.transform.SetParent(null);
        }
    }

    // This method is called to start the lift's movement
    void StartLift()
    {
        hasStartedMoving = true;
        StartCoroutine(MoveLift());
    }

    IEnumerator MoveLift()
    {
        if (targetPosition == null)
        {
            Debug.LogError("PlatformLift: Target Position not assigned! Please assign an empty GameObject as the target.", this);
            // Optionally unparent player if moving without target
            if (playerIsCurrentlyOnPlatform && transform.childCount > 0)
            {
                Transform playerChild = transform.GetChild(0); // Assuming player is the only child
                if (playerChild.CompareTag("Player"))
                {
                    playerChild.SetParent(null);
                }
            }
            yield break;
        }

        // Move towards the target position
        while (Vector3.Distance(transform.position, targetPosition.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, moveSpeed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

        transform.position = targetPosition.position; // Snap to target to ensure exact position

        // If it's a one-way lift, it stays at the top.
        if (oneWayLift)
        {
            // The lift remains at the top. If player is still on it, they remain parented.
            // If you need the lift to be reusable, you'd unparent here and potentially reset hasStartedMoving later.
        }
        else
        {
            // Added logic for the lift to return to its initial position
            yield return new WaitForSeconds(2f); // Wait for 2 seconds at the top (changed from 0.5f)

            // Move back towards the initial position
            while (Vector3.Distance(transform.position, initialPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, initialPosition, moveSpeed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }
            transform.position = initialPosition; // Snap to initial position

            // Reset state so it can be used again
            hasStartedMoving = false;
            playerIsCurrentlyOnPlatform = false; // Player is likely off by now, but reset just in case
        }
    }
}
