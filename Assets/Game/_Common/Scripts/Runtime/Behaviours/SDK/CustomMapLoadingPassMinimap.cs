using UnityEngine;
using UnityEngine.SceneManagement;
using DanielLochner.Assets;
using UnityEngine.UI;

public class CustomMapLoadingPassMinimap : CustomMapLoadingPass
{
    [SerializeField] private RawImage minimapRawImg;
    [SerializeField] private MinimapManager minimapManager;

	public override void Load(Scene scene)
	{
        if (scene.TryGetComponent(out MapInfo info))
        {
            minimapRawImg.texture = info.minimapImage;
            minimapManager.mapBounds = new Rect(info.transform.position.x, info.transform.position.z, info.minimapSize, info.minimapSize);
        }
    }
}