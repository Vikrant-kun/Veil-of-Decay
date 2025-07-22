using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Transform cam;
    private Vector3 previousCamPos;
    public float parallaxEffectMultiplier; // 0.1 = slow, 1 = full cam speed

    private void Start()
    {
        cam = Camera.main.transform;
        previousCamPos = cam.position;
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = cam.position - previousCamPos;
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier, deltaMovement.y * parallaxEffectMultiplier, 0f);
        previousCamPos = cam.position;
    }
}
