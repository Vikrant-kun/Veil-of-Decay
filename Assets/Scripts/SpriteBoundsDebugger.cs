using UnityEngine;

[ExecuteAlways] // So it runs even in edit mode
public class SpriteBoundsDebugger : MonoBehaviour
{
    public Vector3 center;
    public Vector3 size;

    private void Update()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Bounds bounds = sr.bounds;
            center = bounds.center;
            size = bounds.size;
        }
    }
}
