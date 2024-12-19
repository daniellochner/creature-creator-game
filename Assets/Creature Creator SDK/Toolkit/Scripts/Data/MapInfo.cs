
using UnityEngine;

public class MapInfo : MonoBehaviour
{
    public PlatformProxy[] platformProxies;
    public UnlockableBodyPartProxy[] unlockableBodyPartProxies;
    public UnlockablePatternProxy[] unlockablePatternProxies;

    public void OnValidate()
    {
        platformProxies = FindObjectsOfType<PlatformProxy>();
        unlockableBodyPartProxies = FindObjectsOfType<UnlockableBodyPartProxy>();
        unlockablePatternProxies = FindObjectsOfType<UnlockablePatternProxy>();
    }
}