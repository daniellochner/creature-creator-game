
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DanielLochner.Assets.CreatureCreator
{
    public class CustomMapSceneLoader : MonoBehaviour
    {
        public GameObject world;
        public GameObject error;
        public TextMeshProUGUI errorText;

        private void OnEnable()
        {
            CustomMapLoader.OnCustomMapLoaded += OnCustomMapLoaded;
            CustomMapLoader.OnCustomMapLoadFailed += OnCustomMapLoadFailed;
        }

        private void OnDisable()
        {
            CustomMapLoader.OnCustomMapLoaded -= OnCustomMapLoaded;
            CustomMapLoader.OnCustomMapLoadFailed -= OnCustomMapLoadFailed;
        }

        private void Start()
        {
            if (CustomMapLoader.IsCustomMapLoadedFromSDK)
            {
                CustomMapLoader.Load(CustomMapLoader.CustomMapPath);
            }
            else
            {
                CustomMapLoader.Load(Path.Combine(CCConstants.MapsDir, WorldManager.Instance.World.CustomMapId));
            }
        }

        private void OnCustomMapLoaded(Scene scene)
        {
            foreach (var pass in GetComponentsInChildren<CustomMapLoadingPass>())
            {
                pass.Load(scene);
            }
            world.SetActive(true);
        }
        private void OnCustomMapLoadFailed(string reason)
        {
            error.SetActive(true);
            errorText.text = reason;
        }
    }
}