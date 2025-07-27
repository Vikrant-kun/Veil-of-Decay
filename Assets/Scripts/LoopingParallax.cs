using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public Transform[] tiles;
    public float parallaxMultiplier = 0.5f;
    public float verticalMultiplier = 0f; // Optional vertical movement
}

public class LoopingParallax : MonoBehaviour
{
    public ParallaxLayer[] layers;

    private Transform cam;
    private Vector3 lastCamPos;

    private void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    private void LateUpdate()
    {
        Vector3 camDelta = cam.position - lastCamPos;

        foreach (ParallaxLayer layer in layers)
        {
            foreach (Transform tile in layer.tiles)
            {
                tile.position += new Vector3(
                    camDelta.x * layer.parallaxMultiplier,
                    camDelta.y * layer.verticalMultiplier,
                    0f
                );
            }

            HandleLooping(layer);
        }

        lastCamPos = cam.position;
    }

    private void HandleLooping(ParallaxLayer layer)
    {
        if (layer.tiles.Length < 2) return;

        float viewZone = GetSpriteWidth(layer.tiles[0]) / 2f;
        Transform leftMost = layer.tiles[0];
        Transform rightMost = layer.tiles[layer.tiles.Length - 1];

        if (cam.position.x > rightMost.position.x - viewZone)
        {
            leftMost.position = new Vector3(
                rightMost.position.x + GetSpriteWidth(rightMost),
                leftMost.position.y,
                leftMost.position.z
            );
            ShiftTilesRight(layer.tiles);
        }
        else if (cam.position.x < leftMost.position.x + viewZone)
        {
            rightMost.position = new Vector3(
                leftMost.position.x - GetSpriteWidth(rightMost),
                rightMost.position.y,
                rightMost.position.z
            );
            ShiftTilesLeft(layer.tiles);
        }
    }

    private float GetSpriteWidth(Transform t)
    {
        SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
        return sr != null ? sr.bounds.size.x : 0f;
    }

    private void ShiftTilesRight(Transform[] tiles)
    {
        Transform temp = tiles[0];
        for (int i = 0; i < tiles.Length - 1; i++)
            tiles[i] = tiles[i + 1];
        tiles[tiles.Length - 1] = temp;
    }

    private void ShiftTilesLeft(Transform[] tiles)
    {
        Transform temp = tiles[tiles.Length - 1];
        for (int i = tiles.Length - 1; i > 0; i--)
            tiles[i] = tiles[i - 1];
        tiles[0] = temp;
    }
}
