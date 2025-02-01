using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CustomMapRequiredComponentsValidator
{
	public static bool IsSceneValid(Scene scene, out string error)
	{
		List<MapInfo> mapInfos = new List<MapInfo>();

		foreach(var root in scene.GetRootGameObjects())
		{
			foreach(var mapInfo in root.GetComponentsInChildren<MapInfo>())
			{
				mapInfos.Add(mapInfo);
			}
		}

		if(mapInfos.Count > 1)
		{
			error = $"Only 1 MapInfo can be present in the scene. This map has {mapInfos.Count}.";
			return false;
		}

		if(mapInfos.Count == 0)
		{
			error = $"Missing required active component MapInfo. You should have at least 1 of these in your map on an active GameObject.";
			return false;
		}

		error = "";
		return true;
	}
}
