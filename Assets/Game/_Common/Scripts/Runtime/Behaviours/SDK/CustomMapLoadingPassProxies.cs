using DanielLochner.Assets.CreatureCreator;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomMapLoadingPassProxies : CustomMapLoadingPass
{
	[SerializeField] private Platform platformPrefab;
    [SerializeField] private GameSetup gameSetup;
    [SerializeField] private Transform worldRoot;

	public override void Load(Scene scene)
	{
        if (scene.TryGetComponent(out MapInfo info))
        {
            List<Platform> platforms = new List<Platform>();
            foreach (var proxy in info.platformProxies)
            {
                var platform = Instantiate(platformPrefab, proxy.transform.position, proxy.transform.rotation, worldRoot);
                platform.transform.up = Vector3.up;
                platforms.Add(platform);
                Destroy(proxy.gameObject);
            }
            gameSetup.SetPlatforms(platforms.ToArray());
        }
    }
}