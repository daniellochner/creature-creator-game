
using UnityEngine;

[ExecuteInEditMode]
public class MapInfo : MonoBehaviour
{
    [Header("Minimap")]
    public Texture minimapImage;
    public float minimapSize;

    [Header("Proxies")]
    public PlatformProxy[] platformProxies;
    public UnlockableBodyPartProxy[] unlockableBodyPartProxies;
    public UnlockablePatternProxy[] unlockablePatternProxies;

    public bool IsValidMinimap => minimapImage != null && minimapSize > 0;

#if UNITY_EDITOR
    private void Update()
    {
        transform.GetChild(0).gameObject.SetActive(UnityEditor.Selection.activeGameObject == gameObject && IsValidMinimap);
    }

    public void OnValidate()
    {
        platformProxies = FindObjectsOfType<PlatformProxy>();
        unlockableBodyPartProxies = FindObjectsOfType<UnlockableBodyPartProxy>();
        unlockablePatternProxies = FindObjectsOfType<UnlockablePatternProxy>();
    }
#endif
}