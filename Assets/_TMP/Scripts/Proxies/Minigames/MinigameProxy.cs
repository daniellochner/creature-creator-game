using UnityEngine;

public class MinigameProxy : ProxyBehaviour
{
    [Header("Minigame")]
    public int waitTime = 30;
    public int minPlayers = 2;
    public int maxPlayers = 12;
    [Space]
    public CinematicCameraProxy cinematicCameraProxy;
    public Vector3 zoneSize = new Vector3(25f, 10f, 25f);
    [Space]
    public int buildTime = 60;
    [Space]
    public int playTime = 120;
    public bool enablePVP;
    public bool isAscendingOrder;

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.up * (zoneSize.y / 2f), zoneSize);
    }
}
