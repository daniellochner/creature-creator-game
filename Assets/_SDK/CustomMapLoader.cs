using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CustomMapLoader
{
	public static bool IsCustomMapLoaded { get; private set; }
    public static bool IsUsingSDK { get; set; }

	//public static WorkshopMap LoadedCustomMap { get; private set; }

	// we can't load the same asset bundle twice, or it gives an error.
	// so we store the ones already loaded so that we can unload them
	// again if needed (i.e. loading a map twice). I unload instead of reusing
	// so that if a map is updated while we are playing we will still have the new version.
	static Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();

	public static event Action<Scene> OnCustomMapLoaded;
	public static event Action<string> OnCustomMapLoadFailed;
	public static event Action OnCustomMapUnloaded;

	static Scene loadedScene;

	public static void Load(string path)
	{
		try
		{
			if(path == null)
			{
				OnCustomMapLoadFailed?.Invoke($"Failed to load Custom Map: path was null");
				return;
			}

			//Debug.Log($"Custom map load started from {path}");
			string assetBundleFolderName = "Bundles";

			// Get the asset bundles in the folder
			DirectoryInfo directory = new DirectoryInfo(Path.Combine(path, assetBundleFolderName));

			FileInfo[] files = directory.GetFiles();

			List<AssetBundle> bundles = new List<AssetBundle>();

			// Load all the asset bundles
			foreach(FileInfo file in files)
			{
				// skip the manifest file, we don't need to load it
				if(file.Name == assetBundleFolderName)
					continue;

				if(loadedAssetBundles.ContainsKey(file.FullName))
				{
					loadedAssetBundles[file.FullName].Unload(true);
				}

				try
				{
					AssetBundle assetBundle = AssetBundle.LoadFromFile(file.FullName);
					loadedAssetBundles[file.FullName] = assetBundle;

					if(assetBundle == null)
					{
						OnCustomMapLoadFailed?.Invoke($"Failed to load AssetBundle: {file.FullName}");
						Debug.LogError($"Failed to load AssetBundle: {file.FullName}");
						AssetBundle.UnloadAllAssetBundles(true);
						return;
					}
					else
					{
						bundles.Add(assetBundle);
						//Debug.Log($"Loaded AssetBundle: {file.FullName}");
					}
				}
				catch (Exception ex) // this might be pointless: https://stackoverflow.com/questions/73251491/unity-try-catch-with-assetbundle-loadfromfile-does-not-work but maybe it works in the built game?
				{
					OnCustomMapLoadFailed?.Invoke($"Failed to load Custom Map: {ex.Message}");
					AssetBundle.UnloadAllAssetBundles(true);
					return;
				}
			}

			// Load the map scene
			foreach(AssetBundle bundle in bundles)
			{
				if(bundle.name.EndsWith("_scene"))
				{
					string[] scenePaths = bundle.GetAllScenePaths();

					if(scenePaths.Length == 0)
					{
						OnCustomMapLoadFailed?.Invoke($"Failed to load Custom Map: There was no scene in the {bundle.name} AssetBundle");
						return;
					}

					if(scenePaths.Length > 1)
					{
						OnCustomMapLoadFailed?.Invoke($"Failed to load Custom Map: There was more than one scene in the {bundle.name} AssetBundle");
						return;
					}

					string scenePath = scenePaths[0];

					SceneManager.sceneLoaded += OnSceneLoaded;
					SceneManager.LoadScene(scenePath, LoadSceneMode.Additive);
					return;
				}
			}

			OnCustomMapLoadFailed?.Invoke("Error loading custom map: No map asset bundle found");
			Debug.LogError("Error loading custom map: No map asset bundle found");
		}
		catch(Exception ex)
		{
			OnCustomMapLoadFailed?.Invoke($"Error loading custom map: {ex.Message}");
			Debug.LogError("Error loading custom map: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if(mode != LoadSceneMode.Additive)
		{
			return;
		}

		loadedScene = scene;

		SceneManager.SetActiveScene(scene);
		SceneManager.sceneLoaded -= OnSceneLoaded;

		if(!CustomMapValidator.IsSceneValid(scene, out string error))
		{
			Debug.LogError("Map is not valid!");
			foreach(var go in scene.GetRootGameObjects())
			{
				GameObject.DestroyImmediate(go);
			}
			SceneManager.UnloadSceneAsync(scene);

			OnCustomMapLoadFailed?.Invoke(error);
			return;
		}

		CustomMapSecurityValidator.SanitizeAnimators(scene);

		IsCustomMapLoaded = true;
		OnCustomMapLoaded?.Invoke(scene);
	}

	// this should be called manually
	// by anything that disconnects from
	// the scene
	public static void TryUnload()
	{
		if(!IsCustomMapLoaded)
			return;

		// "obsolete" because you cant call it from physics or visual callbacks.
		// its fine to use on a button press
		SceneManager.UnloadScene(loadedScene);

		foreach(var loaded in loadedAssetBundles)
		{
			loaded.Value.Unload(true);
		}

		loadedAssetBundles.Clear();

		OnCustomMapUnloaded?.Invoke();
		IsCustomMapLoaded = false;
	}
}