using UnityEngine;
using UnityEngine.SceneManagement;
using DanielLochner.Assets;

public class CustomMapLoadingPassLayers : CustomMapLoadingPass
{
    [SerializeField] private Transform worldRoot;

	public override void Load(Scene scene)
	{
        foreach (var setLayer in worldRoot.GetComponentsInChildren<SetLayer>(true))
        {
            int layerIndex = LayerMask.NameToLayer(setLayer.layerName);
            if (layerIndex != -1)
            {
                if (setLayer.includeChildren)
                {
                    setLayer.gameObject.SetLayerRecursively(layerIndex);
                }
                else
                {
                    setLayer.gameObject.layer = layerIndex;
                }
            }
        }
    }
}