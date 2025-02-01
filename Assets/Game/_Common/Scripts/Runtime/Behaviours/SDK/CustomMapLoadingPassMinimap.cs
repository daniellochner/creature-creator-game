using UnityEngine;
using UnityEngine.SceneManagement;
using DanielLochner.Assets;
using UnityEngine.UI;

public class CustomMapLoadingPassMinimap : CustomMapLoadingPass
{
    [SerializeField] private RawImage minimapRawImg;
    [SerializeField] private MinimapManager minimapManager;
    [SerializeField] private Menu minimapMenu;

    public override void Load(Scene scene)
    {
        if (scene.TryGetComponent(out MapInfo info))
        {
            if (info.IsValidMinimap)
            {
                minimapRawImg.texture = info.minimapImage;
                minimapManager.mapBounds = new Rect(info.transform.position.x, info.transform.position.z, info.minimapSize, info.minimapSize);
            }
            else
            {
                minimapMenu.gameObject.SetActive(false);
            }
        }
    }
}