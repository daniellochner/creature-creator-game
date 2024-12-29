using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class CustomMapLoadingPass : MonoBehaviour
{
	public abstract void Load(Scene scene);
}