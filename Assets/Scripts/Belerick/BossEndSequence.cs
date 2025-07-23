using UnityEngine;

public class BossEndSequence : MonoBehaviour
{
    public Transform triangleGate;
    public Vector3 openPosition = new Vector3(0, 18.1f, 0); // Change X/Z as per your gate’s position
    public float liftSpeed = 1f;

    public GameObject demonGatePrefab;
    public GameObject angelPrefab;

    public Transform demonSpawnPoint;
    public Transform angelSpawnPoint;

    private bool gateOpening = false;

    public void StartGateSequence()
    {
        gateOpening = true;
    }

    void Update()
    {
        if (gateOpening && triangleGate != null)
        {
            triangleGate.position = Vector3.MoveTowards(
                triangleGate.position,
                openPosition,
                liftSpeed * Time.deltaTime
            );

            if (Vector3.Distance(triangleGate.position, openPosition) < 0.01f)
            {
                gateOpening = false;
                SpawnGateAndAngel();
            }
        }
    }

    void SpawnGateAndAngel()
    {
        Instantiate(demonGatePrefab, demonSpawnPoint.position, Quaternion.identity);
        Instantiate(angelPrefab, angelSpawnPoint.position, Quaternion.identity);
    }
}
