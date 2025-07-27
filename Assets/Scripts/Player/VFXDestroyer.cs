// VFXDestroyer.cs
using UnityEngine;

public class VFXDestroyer : MonoBehaviour
{
    // This function should be called via an Animation Event at the end of the VFX animation.
    public void DestroyVFXGameObject()
    {
        Destroy(gameObject); // Destroys the GameObject this script is attached to.
    }
}