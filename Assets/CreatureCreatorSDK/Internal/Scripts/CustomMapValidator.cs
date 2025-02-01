using UnityEngine.SceneManagement;

public static class CustomMapValidator
{
	public static bool IsSceneValid(Scene scene, out string error)
	{
		if(!CustomMapSecurityValidator.IsSceneValid(scene, out error))
		{
			return false;
		}

		if(!CustomMapRequiredComponentsValidator.IsSceneValid(scene, out error))
		{
			return false;
		}

		if(!CustomMapErrorValidator.IsSceneValid(scene, out error))
		{
			return false;
		}

		return true;
	}
}
