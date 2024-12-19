using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUtility
{
    public static bool TryGetComponent<T>(this Scene scene, out T component) where T : Component
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var c = root.GetComponentInChildren<T>(true);
            if (c != null)
            {
                component = c;
                return true;
            }
        }
        component = null;
        return false;
    }
}
