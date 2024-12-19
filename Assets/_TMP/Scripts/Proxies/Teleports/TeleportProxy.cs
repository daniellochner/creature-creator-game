using UnityEngine;

public class TeleportProxy : ProxyBehaviour
{
    public Map targetMap;
    public Vector3 size;

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + Vector3.up * (size.y / 2f), size);
    }

    public enum Map
    {
        Island,
        Farm,
        Sandbox,
        Cave,
        City
    }
}
