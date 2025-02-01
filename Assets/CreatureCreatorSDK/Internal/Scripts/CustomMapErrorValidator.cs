using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine.SceneManagement;

public static class CustomMapErrorValidator
{
	public static bool IsSceneValid(Scene scene, out string error)
	{
		Dictionary<ErrorCollectionBehaviour, List<string>> errorsPerGameObject = new Dictionary<ErrorCollectionBehaviour, List<string>>();

		foreach(var root in scene.GetRootGameObjects())
		{
			foreach(var errorChecker in root.GetComponentsInChildren<ErrorCollectionBehaviour>(true))
			{
				if(!errorsPerGameObject.ContainsKey(errorChecker))
				{
					errorsPerGameObject[errorChecker] = new List<string>();
				}

				List<string> errors = errorsPerGameObject[errorChecker];

				errorChecker.RunErrorChecks(ref errors);

				errorsPerGameObject[errorChecker] = errors;
			}
		}

		var sb = new StringBuilder();

		bool first = true;

		foreach(var kvp in errorsPerGameObject)
		{
			foreach(var err in kvp.Value)
			{
#if UNITY_EDITOR
				if(first)
				{
					Selection.activeGameObject = kvp.Key.gameObject;
					EditorGUIUtility.PingObject(kvp.Key.gameObject);
					first = false;
				}
#endif

				sb.AppendLine(kvp.Key + ": " + err);
				sb.AppendLine();
			}
		}

		error = sb.ToString();

		return string.IsNullOrEmpty(error);
	}
}