using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public static class CustomMapSecurityValidator
{
	static Type[] whitelistedComponents = {
		typeof(Transform),
		typeof(Collider),
		typeof(MeshFilter),
		typeof(Renderer),
		typeof(AudioSource),
		typeof(AudioReverbZone),
		typeof(AudioListener),
		typeof(ReflectionProbe),
		typeof(LightProbes),
		typeof(LightProbeGroup),
		typeof(ParticleSystem),
		typeof(ParticleSystemForceField),
		typeof(TrailRenderer),
		typeof(LineRenderer),
		typeof(Canvas),
		typeof(CanvasScaler),
		typeof(TextMeshProUGUI),
		typeof(CanvasRenderer),
		typeof(Camera),
		typeof(Light),
		typeof(MapInfo),
		typeof(Animator),
		typeof(Activator),
		typeof(Text),
		typeof(Rigidbody),
		typeof(Outline),
		typeof(Shadow),
		typeof(Image),
		typeof(RawImage),
		typeof(VerticalLayoutGroup),
		typeof(HorizontalLayoutGroup),
		typeof(CanvasGroup),
		typeof(TMP_Text),
		typeof(Terrain),
		typeof(PlatformProxy),
        typeof(SetLayer)
	};

	public static bool IsGameObjectValid(GameObject go, out string error)
	{
		foreach(var component in go.GetComponentsInChildren(typeof(Component), true))
		{
			// can't be illegal if it doesn't exist
			if(component == null)
			{
				continue;
			}

			bool isSafe = false;

			// go through all whitelisted components
			// if it matches a single one, we're ok!
			foreach(var type in whitelistedComponents)
			{
				if(type.IsInstanceOfType(component))
				{
					isSafe = true;
					break;
				}
			}

			if(!isSafe)
			{
				error = $"{component.gameObject} contains a non-whitelisted component ({component.GetType()})";
				return false;
			}
		}

		error = "";
		return true;
	}

	public static bool IsSceneValid(Scene scene, out string error)
	{
		foreach(var root in scene.GetRootGameObjects())
		{
			if(!IsGameObjectValid(root, out error))
			{
				return false;
			}
		}

		error = "";
		return true;
	}

	public static void SanitizeAnimators(Scene scene)
	{
		foreach(var root in scene.GetRootGameObjects())
		{
			foreach(Animator animator in root.GetComponentsInChildren(typeof(Animator), true))
			{
				animator.fireEvents = false;

				if(animator.runtimeAnimatorController != null)
				{
					foreach(var clip in animator.runtimeAnimatorController.animationClips)
					{
						if(clip.events.Length > 0)
						{
							Debug.Log($"Animator on GameObject {animator.gameObject.name} had events which were removed.");
							clip.events = new AnimationEvent[0];
						}
					}
				}
			}
		}
	}
}