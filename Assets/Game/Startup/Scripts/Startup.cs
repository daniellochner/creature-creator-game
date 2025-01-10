using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace DanielLochner.Assets.CreatureCreator
{
    public class Startup : MonoBehaviour
    {
        #region Fields
        [SerializeField] private int baseWidth;
        [SerializeField] private float scale;
        [SerializeField] private float speed;
        [SerializeField] private Material gridMaterial;
        [SerializeField] private Animator logoAnimator;
        [SerializeField] private AudioSource enterAudioSource;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI eduLinkText;
        [SerializeField] private LocalizedText copyrightText;
        [SerializeField] private TMP_InputField institutionIdInputField;

        private string currentPromptId;
        #endregion

        #region Properties
        private bool ShowIntro
        {
            get => PlayerPrefs.GetInt("SHOW_INTRO", 1) == 1;
            set => PlayerPrefs.SetInt("SHOW_INTRO", value ? 1 : 0);
        }
        #endregion

        #region Methods
        private IEnumerator Start()
        {
            float n = (float)baseWidth / Screen.width;
            float s = 1f / scale;
            gridMaterial.mainTextureScale = (n * s) * new Vector2(Screen.width, Screen.height);

            yield return LocalizeRoutine();
            yield return LinkRoutine();
            yield return AuthenticateRoutine();
            yield return EnterRoutine();
        }
        private void Update()
        {
            gridMaterial.mainTextureOffset -= speed * Time.deltaTime * Vector2.one;

            if (EducationManager.Instance.IsEducational)
            {
                eduLinkText.gameObject.SetActive(Input.GetKey(KeyCode.Tab));
            }
        }
        private void OnDestroy()
        {
            gridMaterial.mainTextureScale = Vector2.one;
            gridMaterial.mainTextureOffset = Vector2.zero;

            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
        
        private IEnumerator LocalizeRoutine()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            copyrightText.SetArguments(DateTime.Now.Year);
            yield return new WaitUntil(() => LocalizationSettings.InitializationOperation.IsDone);
        }
        private IEnumerator LinkRoutine()
        {
            if (EducationManager.Instance.IsEducational)
            {
                eduLinkText.text = SystemInfo.deviceUniqueIdentifier;

                while (!EducationManager.Instance.IsLinked)
                {
                    SetPrompt(null);
                    SetInstitutionIdInputField(true);
                    yield return new WaitUntil(() => !string.IsNullOrEmpty(institutionIdInputField.text) && Input.GetKeyDown(KeyCode.Return));
                    SetInstitutionIdInputField(false);

                    SetPromptId("startup_linking");
                    yield return EducationManager.Instance.LinkRoutine(institutionIdInputField.text, (bool isLinked, string response) => SetPrompt(response));
                    yield return new WaitForSeconds(2.5f);
                }

                eduLinkText.text = $"{SystemInfo.deviceUniqueIdentifier} - {EducationManager.Instance.InstitutionId}";
            }
        }
        private IEnumerator AuthenticateRoutine()
        {
            AuthenticationManager.Instance.Authenticate();
            while (AuthenticationManager.Instance.Status != AuthenticationManager.AuthStatus.Success)
            {
                SetPromptId("startup_authenticating");

                yield return new WaitUntil(() => AuthenticationManager.Instance.Status != AuthenticationManager.AuthStatus.Busy);

                if (AuthenticationManager.Instance.Status == AuthenticationManager.AuthStatus.Fail)
                {
                    SetPromptId("startup_failed-to-authenticate");
                    yield return new WaitUntil(() => Input.anyKeyDown && !CanvasUtility.IsPointerOverUI);
                    AuthenticationManager.Instance.Authenticate();
                }
            }

#if UNITY_IOS
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif
        }
        private IEnumerator EnterRoutine()
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();

            bool load = false;
            bool upload = false;
            string path = "";
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                string arg = commandLineArgs[i];
                switch (arg)
                {
                    case "-loadmap":
                        load = true;
                        break;

                    case "-uploadmap":
                        upload = true;
                        break;
                }

                if (load || upload)
                {
                    CustomMapLoader.CustomMapPath = path = commandLineArgs[i + 1];
                    break;
                }
            }

            if (load)
            {
                // Setup World
                Map map = Map.Custom;
                Mode mode = Mode.Creative;
                bool spawnNPC = true;
                bool enablePVE = true;
                bool unlimited = false;
                WorldManager.Instance.World = new WorldSP(map, mode, spawnNPC, enablePVE, unlimited);

                // Set Connection Data
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = NetworkTransportPicker.Instance.GetTransport<UnityTransport>("localhost");
                NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new ConnectionData("", "", "", ProgressManager.Data.Level)));

                // Start
                NetworkManager.Singleton.StartHost();
            }
            else
            if (upload)
            {
                string previewPath = Path.Combine(path, "thumb.png");
                string configPath = Path.Combine(path, "config.json");
                MapConfigData configData = SaveUtility.Load<MapConfigData>(configPath);

                if (ulong.TryParse(configData.ItemId, out ulong itemId))
                {
                    // Update
                    SetPromptId("startup_updating", 0);
                    Action<float> onProgress = delegate (float p)
                    {
                        SetPromptId("startup_updating", Math.Round(p * 100f, 2));
                    };
                    Action<string> onUpdated = delegate (string id)
                    {
                        SetPromptId("startup_updated");
                    };
                    Action<string> onFailed = delegate (string reason)
                    {
                        SetPromptId("startup_failed");
                    };

                    yield return FactoryManager.Instance.UpdateItemRoutine(ulong.Parse(configData.ItemId), configData.Name, configData.Description, FactoryItemType.Map, path, previewPath, onProgress, onUpdated, onFailed);
                }
                else
                {
                    // Upload
                    SetPromptId("startup_uploading", 0);
                    Action<float> onProgress = delegate (float p)
                    {
                        SetPromptId("startup_uploading", Math.Round(p * 100f, 2));
                    };
                    Action<string> onUploaded = delegate (string id)
                    {
                        SetPromptId("startup_uploaded");
                        configData.ItemId = id; // Update the item's Id
                        SaveUtility.Save(configPath, configData);
                    };
                    Action<string> onFailed = delegate (string reason)
                    {
                        SetPromptId("startup_failed");
                    };

                    yield return FactoryManager.Instance.UploadItemRoutine(configData.Name, configData.Description, FactoryItemType.Map, path, previewPath, onProgress, onUploaded, onFailed);
                }

                // Wait
                yield return new WaitUntil(() => Input.anyKeyDown && !CanvasUtility.IsPointerOverUI);
                Application.Quit();
            }
            else
            {
                SetPromptId(SystemUtility.IsDevice(DeviceType.Handheld) ? "startup_tap-to-start" : "startup_press-any-button");
                yield return new WaitUntil(() => Input.anyKeyDown && !CanvasUtility.IsPointerOverUI);

                if (ShowIntro)
                {
                    Fader.FadeInOut(1f, delegate
                    {
                        SceneManager.LoadScene("Intro");
                        ShowIntro = false;
                    });
                }
                else
                {
                    LoadingManager.Instance.Load("MainMenu");
                }
                OnEntered();
            }
        }

        private void SetInstitutionIdInputField(bool isActive)
        {
            institutionIdInputField.gameObject.SetActive(isActive);
        }
        private void SetPromptId(string promptId, params object[] args)
        {
            promptText.text = LocalizationUtility.Localize(currentPromptId = promptId, args);
        }
        private void SetPrompt(string prompt)
        {
            promptText.text = prompt;
            currentPromptId = null;
        }

        private void OnEntered()
        {
            MusicManager.Instance.FadeTo(null);
            logoAnimator.SetTrigger("Hide");
            enterAudioSource.Play();
        }
        private void OnLocaleChanged(Locale locale)
        {
            if (currentPromptId != null)
            {
                SetPromptId(currentPromptId);
            }
        }
        #endregion
    }
}