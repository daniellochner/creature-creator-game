
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

    public bool IsValidMinimap => (minimapImage != null) && (minimapSize > 0);

#if UNITY_EDITOR
    private void Start()
    {
        Setup();
    }
    private void Update()
    {
        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(UnityEditor.Selection.activeGameObject == gameObject && IsValidMinimap);
        }
    }
#endif

    public void Setup()
    {
        SetupProxies();
    }
    public void SetupProxies()
    {
        platformProxies = FindObjectsOfType<PlatformProxy>();
        unlockableBodyPartProxies = FindObjectsOfType<UnlockableBodyPartProxy>();
        unlockablePatternProxies = FindObjectsOfType<UnlockablePatternProxy>();
    }
}