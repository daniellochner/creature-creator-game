using DanielLochner.Assets.CreatureCreator;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomMapLoadingPassPlatforms : CustomMapLoadingPass
{
	[SerializeField] private Platform platformPrefab;
    [SerializeField] private GameSetup gameSetup;

	public override void Load(Scene scene)
	{
        if (scene.TryGetComponent(out MapInfo info))
        {
            List<Platform> platforms = new List<Platform>();
            foreach (var proxy in info.platformProxies)
            {
                var platform = Instantiate(platformPrefab, proxy.transform.position, proxy.transform.rotation);
                platforms.Add(platform);
                Destroy(proxy.gameObject);
            }
            gameSetup.SetPlatforms(platforms.ToArray());
        }
    }
}