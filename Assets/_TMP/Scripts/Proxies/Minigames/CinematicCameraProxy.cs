using UnityEngine;

public class CinematicCameraProxy : ProxyBehaviour
{
    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, 60, 1000f, 0.3f, ((float)Screen.width) / Screen.height);
    }
}
