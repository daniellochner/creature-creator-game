// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using Crosstales.FB;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using static DanielLochner.Assets.CreatureCreator.Settings;

namespace DanielLochner.Assets.CreatureCreator
{
    public class SettingsUI : MonoBehaviour
    {
        #region Fields
        [SerializeField] private bool inGame;

        [Header("Video")]
        [SerializeField] private OptionSelector resolutionOS;
        [SerializeField] private GameObject resolutionApply;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle vSyncToggle;
        [SerializeField] private Slider targetFrameRateSlider;
        [SerializeField] private Slider screenScaleSlider;
        [SerializeField] private GameObject screenScaleApply;
        [SerializeField] private OptionSelector presetOS;
        [SerializeField] private OptionSelector creatureMeshQualityOS;
        [SerializeField] private OptionSelector shadowQualityOS;
        [SerializeField] private OptionSelector textureQualityOS;
        [SerializeField] private OptionSelector ambientOcclusionOS;
        [SerializeField] private OptionSelector antialiasingOS;
        [SerializeField] private OptionSelector screenSpaceReflectionsOS;
        [SerializeField] private OptionSelector foliageOS;
        [SerializeField] private Toggle ambientParticlesToggle;
        [SerializeField] private Toggle reflectionsToggle;
        [SerializeField] private Toggle anisotropicFilteringToggle;
        [SerializeField] private Toggle bloomToggle;
        [SerializeField] private Toggle depthOfFieldToggle;
        [SerializeField] private Toggle motionBlurToggle;
        [SerializeField] private Toggle optimizeCreaturesToggle;
        [SerializeField] private Toggle optimizeLightingToggle;
        [SerializeField] private ParticleSystem[] ambientParticles;
        [SerializeField] private Slider creatureRenderDistanceSlider;

        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider soundEffectsVolumeSlider;
        [SerializeField] private OptionSelector inGameMusicOS;

        [Header("Gameplay")]
        [SerializeField] private TextMeshProUGUI creaturePresetsText;
        [SerializeField] private Slider exportPrecisionSlider;
        [SerializeField] private Button creaturePresetsButton;
        [SerializeField] private Toggle exportAllToggle;
        [SerializeField] private Toggle cameraShakeToggle;
        [SerializeField] private Toggle vibrationsToggle;
        [SerializeField] private Toggle debugModeToggle;
        [SerializeField] private Toggle previewFeaturesToggle;
        [SerializeField] private Toggle networkStatsToggle;
        [SerializeField] private Toggle tutorialToggle;
        [SerializeField] private Toggle worldChatToggle;
        [SerializeField] private Toggle mapToggle;
        [SerializeField] private Toggle footstepsToggle;
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private GameObject selectFilesGO;
        [SerializeField] private GameObject clearFilesGO;

        [Header("Controls")]
        [SerializeField] private Slider sensitivityHorizontalSlider;
        [SerializeField] private Slider sensitivityVerticalSlider;
        [SerializeField] private Toggle invertHorizontalToggle;
        [SerializeField] private Toggle invertVerticalToggle;
        [SerializeField] private Slider interfaceScaleSlider;
        [SerializeField] private OptionSelector joystickOS;
        [SerializeField] private Slider joystickHorizontalSlider;
        [SerializeField] private Slider joystickVerticalSlider;
        [SerializeField] private CanvasGroup joystickHorizontalCG;
        [SerializeField] private CanvasGroup joystickVerticalCG;
        [SerializeField] private Slider touchOffsetSlider;
        [SerializeField] private Toggle flipButtonToggle;
        [SerializeField] private Toggle zoomButtonsToggle;

        private Coroutine previewMusicCoroutine;
        #endregion

        #region Methods
        private void Start()
        {
            Setup();
        }
        private void OnDestroy()
        {
            Shutdown();
        }
        private void OnEnable()
        {
            if (SystemUtility.IsDevice(DeviceType.Handheld))
            {
                targetFrameRateSlider.value = Application.targetFrameRate;
            }
        }

        private void Setup()
        {
            #region Video
            // Resolution
            for (int i = 0; i < Screen.resolutions.Length; ++i)
            {
                Resolution resolution = Screen.resolutions[i];
                resolutionOS.Options.Add(new OptionSelector.Option()
                {
                    Id = $"{resolution.width}x{resolution.height} @ {resolution.refreshRate}Hz"
                });
                resolutionOS.OnSelected.AddListener(delegate
                {
                    resolutionApply.SetActive(true);
                });

                Resolution current = SettingsManager.Data.Resolution;
                if ((resolution.width == current.width) && (resolution.height == current.height) && (resolution.refreshRate == current.refreshRate))
                {
                    resolutionOS.Select(i, false);
                }
            }

            // Fullscreen
            fullscreenToggle.SetIsOnWithoutNotify(SettingsManager.Data.Fullscreen);
            fullscreenToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetFullscreen(isOn);
            });

            // V-Sync
            vSyncToggle.SetIsOnWithoutNotify(SettingsManager.Data.VSync);
            vSyncToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetVSync(isOn);
            });

            if (SystemUtility.IsDevice(DeviceType.Handheld))
            {
                // Screen Scale
                screenScaleSlider.value = (float)SettingsManager.Data.Resolution.width / Display.main.systemWidth;
                screenScaleSlider.onValueChanged.AddListener(delegate
                {
                    screenScaleApply.SetActive(true);
                });

                // Target Frame Rate
                targetFrameRateSlider.value = SettingsManager.Data.TargetFrameRate;
                targetFrameRateSlider.onValueChanged.AddListener(delegate (float value)
                {
                    SettingsManager.Instance.SetTargetFrameRate((int)value);
                    TieredFramerate.Instance?.Reset();
                });
            }


            // Preset
            presetOS.SetupUsingEnum<PresetType>();
            presetOS.Select(PresetType.Custom, false);
            presetOS.OnSelected.AddListener(delegate (int option)
            {
                PresetType presetType = (PresetType)option;
                switch (presetType)
                {
                    case PresetType.VeryLow:
                        creatureMeshQualityOS.Select(CreatureMeshQualityType.Low);
                        shadowQualityOS.Select(ShadowQualityType.Low);
                        textureQualityOS.Select(TextureQualityType.Low);
                        ambientOcclusionOS.Select(AmbientOcclusionType.None);
                        antialiasingOS.Select(AntialiasingType.None);
                        screenSpaceReflectionsOS.Select(ScreenSpaceReflectionsType.None);
                        foliageOS.Select(FoliageType.VeryLow);
                        reflectionsToggle.isOn = false;
                        anisotropicFilteringToggle.isOn = false;
                        bloomToggle.isOn = false;
                        break;

                    case PresetType.Low:
                        creatureMeshQualityOS.Select(CreatureMeshQualityType.Low);
                        shadowQualityOS.Select(ShadowQualityType.Low);
                        textureQualityOS.Select(TextureQualityType.Low);
                        ambientOcclusionOS.Select(AmbientOcclusionType.None);
                        antialiasingOS.Select(AntialiasingType.FXAA);
                        screenSpaceReflectionsOS.Select(ScreenSpaceReflectionsType.Low);
                        foliageOS.Select(FoliageType.Low);
                        reflectionsToggle.isOn = false;
                        anisotropicFilteringToggle.isOn = false;
                        bloomToggle.isOn = false;
                        break;

                    case PresetType.Medium:
                        creatureMeshQualityOS.Select(CreatureMeshQualityType.Medium);
                        shadowQualityOS.Select(ShadowQualityType.Medium);
                        textureQualityOS.Select(TextureQualityType.Medium);
                        ambientOcclusionOS.Select(AmbientOcclusionType.SAO);
                        antialiasingOS.Select(AntialiasingType.MediumSMAA);
                        screenSpaceReflectionsOS.Select(ScreenSpaceReflectionsType.Medium);
                        foliageOS.Select(FoliageType.Medium);
                        reflectionsToggle.isOn = false;
                        anisotropicFilteringToggle.isOn = true;
                        bloomToggle.isOn = true;
                        break;

                    case PresetType.High:
                        creatureMeshQualityOS.Select(CreatureMeshQualityType.High);
                        shadowQualityOS.Select(ShadowQualityType.High);
                        textureQualityOS.Select(TextureQualityType.High);
                        ambientOcclusionOS.Select(AmbientOcclusionType.MSVO);
                        antialiasingOS.Select(AntialiasingType.HighSMAA);
                        screenSpaceReflectionsOS.Select(ScreenSpaceReflectionsType.High);
                        foliageOS.Select(FoliageType.High);
                        reflectionsToggle.isOn = false;
                        anisotropicFilteringToggle.isOn = true;
                        bloomToggle.isOn = true;
                        break;

                    case PresetType.VeryHigh:
                        creatureMeshQualityOS.Select(CreatureMeshQualityType.High);
                        shadowQualityOS.Select(ShadowQualityType.High);
                        textureQualityOS.Select(TextureQualityType.High);
                        ambientOcclusionOS.Select(AmbientOcclusionType.MSVO);
                        antialiasingOS.Select(AntialiasingType.Temporal);
                        screenSpaceReflectionsOS.Select(ScreenSpaceReflectionsType.VeryHigh);
                        foliageOS.Select(FoliageType.High);
                        reflectionsToggle.isOn = true;
                        anisotropicFilteringToggle.isOn = true;
                        bloomToggle.isOn = true;
                        break;
                }
            });

            // Creature Mesh Quality
            creatureMeshQualityOS.SetupUsingEnum<CreatureMeshQualityType>();
            creatureMeshQualityOS.Select(SettingsManager.Data.CreatureMeshQuality, false);
            creatureMeshQualityOS.OnSelected.AddListener(delegate (int option)
            {
                SettingsManager.Instance.SetCreatureMeshQuality((CreatureMeshQualityType)option);
            });

            // Shadow Quality
            shadowQualityOS.SetupUsingEnum<ShadowQualityType>();
            shadowQualityOS.Select(SettingsManager.Data.ShadowQuality, false);
            shadowQualityOS.OnSelected.AddListener(delegate (int option)
            {
                SettingsManager.Instance.SetShadowQuality((ShadowQualityType)option);
            });

            // Texture Quality
            textureQualityOS.SetupUsingEnum<TextureQualityType>();
            textureQualityOS.Select(SettingsManager.Data.TextureQuality, false);
            textureQualityOS.OnSelected.AddListener(delegate (int option)
            {
                SettingsManager.Instance.SetTextureQuality((TextureQualityType)option);
            });

            // Ambient Occlusion
            ambientOcclusionOS.SetupUsingEnum<AmbientOcclusionType>();
            ambientOcclusionOS.Select(SettingsManager.Data.AmbientOcclusion, false);
            ambientOcclusionOS.OnSelected.AddListener(delegate (int option)
            {
                SettingsManager.Instance.SetAmbientOcclusion((AmbientOcclusionType)option);
            });

            // Antialiasing
            antialiasingOS.SetupUsingEnum<AntialiasingType>();
            antialiasingOS.Select(SettingsManager.Data.Antialiasing, false);
            antialiasingOS.OnSelected.AddListener(delegate (int option)
            {
                SettingsManager.Instance.SetAntialiasing((AntialiasingType)option, true);
            });

            // Screen Space Reflections
            screenSpaceReflectionsOS.SetupUsingEnum<ScreenSpaceReflectionsType>();
            screenSpaceReflectionsOS.Select(SettingsManager.Data.ScreenSpaceReflections, false);
            screenSpaceReflectionsOS.OnSelected.AddListener(delegate (int option)
            {
                SettingsManager.Instance.SetScreenSpaceReflections((ScreenSpaceReflectionsType)option);
            });

            // Foliage
            foliageOS.SetupUsingEnum<FoliageType>();
            foliageOS.Select(SettingsManager.Data.Foliage, false);
            foliageOS.OnSelected.AddListener(delegate (int option)
            {
                SettingsManager.Instance.SetFoliage((FoliageType)option);
            });

            // Ambient Particles
            ambientParticlesToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                foreach (ParticleSystem system in ambientParticles)
                {
                    if (isOn)
                    {
                        system.Play(true);
                    }
                    else
                    {
                        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }

                SettingsManager.Instance.SetAmbientParticles(isOn);
            });
            ambientParticlesToggle.isOn = SettingsManager.Data.AmbientParticles;

            // Reflections
            reflectionsToggle.SetIsOnWithoutNotify(SettingsManager.Data.Reflections);
            reflectionsToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetReflections(isOn);
            });

            // Anisotropic Filtering
            anisotropicFilteringToggle.SetIsOnWithoutNotify(SettingsManager.Data.AnisotropicFiltering);
            anisotropicFilteringToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetAnisotropicFiltering(isOn);
            });

            // Bloom
            bloomToggle.SetIsOnWithoutNotify(SettingsManager.Data.Bloom);
            bloomToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetBloom(isOn);
            });

            // Depth Of Field
            depthOfFieldToggle.SetIsOnWithoutNotify(SettingsManager.Data.DepthOfField);
            depthOfFieldToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetDepthOfField(isOn);
            });

            // Motion Blur
            motionBlurToggle.SetIsOnWithoutNotify(SettingsManager.Data.MotionBlur);
            motionBlurToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetMotionBlur(isOn);
            });

            // Optimize Creatures
            optimizeCreaturesToggle.SetIsOnWithoutNotify(SettingsManager.Data.OptimizeCreatures);
            optimizeCreaturesToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetOptimizeCreatures(isOn);
            });

            // Optimize Lighting
            optimizeLightingToggle.SetIsOnWithoutNotify(SettingsManager.Data.OptimizeLighting);
            optimizeLightingToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetOptimizeLighting(isOn);
            });

            // Creature Render Distance
            creatureRenderDistanceSlider.value = SettingsManager.Data.CreatureRenderDistance;
            creatureRenderDistanceSlider.onValueChanged.AddListener(delegate (float value)
            {
                SettingsManager.Instance.SetCreatureRenderDistance(value);
            });
            #endregion

            #region Audio
            // Master Volume
            masterVolumeSlider.value = SettingsManager.Data.MasterVolume * 100;
            masterVolumeSlider.onValueChanged.AddListener(delegate (float value)
            {
                SettingsManager.Instance.SetMasterVolume(value / 100f);
            });

            // Music Volume
            musicVolumeSlider.value = SettingsManager.Data.MusicVolume * 100;
            musicVolumeSlider.onValueChanged.AddListener(delegate (float value)
            {
                SettingsManager.Instance.SetMusicVolume(value / 100f);
            });

            // Sound Effects Volume
            soundEffectsVolumeSlider.value = SettingsManager.Data.SoundEffectsVolume * 100;
            soundEffectsVolumeSlider.onValueChanged.AddListener(delegate (float value)
            {
                SettingsManager.Instance.SetSoundEffectsVolume(value / 100f);
            });

            // Background Music
            inGameMusicOS.SetupUsingEnum<InGameMusicType>();
            inGameMusicOS.OnSelected.AddListener(delegate (int option)
            {
                InGameMusicType type = (InGameMusicType)option;
                SettingsManager.Instance.SetInGameMusic(type);

                string musicId = SettingsManager.Data.InGameMusicId;
                if (inGame)
                {
                    MusicManager.Instance.FadeTo(musicId);
                }
                else
                {
                    if (previewMusicCoroutine != null)
                    {
                        StopCoroutine(previewMusicCoroutine);
                    }
                    previewMusicCoroutine = StartCoroutine(PreviewMusicRoutine(musicId));
                }
            });
            inGameMusicOS.Select(SettingsManager.Data.InGameMusic, inGame && !WorldManager.Instance.IsUsingTeleport);
            #endregion

            #region Gameplay
            // Creature Preset(s)
            creaturePresetsButton.onClick.AddListener(delegate
            {
                if (SettingsManager.Data.CreaturePresets.Count == 0)
                {
                    if (SystemUtility.IsDevice(DeviceType.Desktop))
                    {
                        FileBrowser.Instance.OpenFilesAsync(true, "dat");
                        FileBrowser.Instance.OnOpenFilesComplete += OnOpenFilesComplete;
                    }
                    else
                    if (SystemUtility.IsDevice(DeviceType.Handheld))
                    {
                        NativeFilePicker.PickMultipleFiles(SelectFiles, NativeFilePicker.AllFileTypes);
                    }
                }
                else
                {
                    SettingsManager.Data.CreaturePresets.Clear();
                    SettingsManager.Instance.Save();

                    UpdatePresetSelectionUI();
                }
            });
            UpdatePresetSelectionUI();

            exportPrecisionSlider.value = SettingsManager.Data.ExportPrecision;
            exportPrecisionSlider.onValueChanged.AddListener(delegate (float precision)
            {
                SettingsManager.Instance.SetExportPrecision((int)precision);
            });

            // Touch Offset
            touchOffsetSlider.value = SettingsManager.Data.TouchOffset;
            touchOffsetSlider.onValueChanged.AddListener(delegate (float value)
            {
                if (inGame)
                {
                    Player.Instance.Editor.TouchOffset = value;
                }
                SettingsManager.Instance.SetTouchOffset((int)value);
            });

            // Export All
            exportAllToggle.SetIsOnWithoutNotify(SettingsManager.Data.ExportAll);
            exportAllToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetExportAll(isOn);
            });

            // Camera Shake
            cameraShakeToggle.SetIsOnWithoutNotify(SettingsManager.Data.CameraShake);
            cameraShakeToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetCameraShake(isOn, true);
            });

            // Vibrations
            vibrationsToggle.SetIsOnWithoutNotify(SettingsManager.Data.Vibrations);
            vibrationsToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetVibrations(isOn);
            });

            // Debug Mode
            debugModeToggle.SetIsOnWithoutNotify(SettingsManager.Data.DebugMode);
            debugModeToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetDebugMode(isOn);
            });

            // Preview Features
            previewFeaturesToggle.SetIsOnWithoutNotify(SettingsManager.Data.PreviewFeatures);
            previewFeaturesToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetPreviewFeatures(isOn);
            });

            // Network Stats
            networkStatsToggle.SetIsOnWithoutNotify(SettingsManager.Data.NetworkStats);
            networkStatsToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetNetworkStats(isOn);
            });

            // Tutorial
            tutorialToggle.SetIsOnWithoutNotify(SettingsManager.Data.Tutorial);
            tutorialToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetTutorial(isOn);
            });

            // World Chat
            worldChatToggle.SetIsOnWithoutNotify(SettingsManager.Data.WorldChat);
            worldChatToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetWorldChat(isOn);
            });

            // Map
            mapToggle.SetIsOnWithoutNotify(SettingsManager.Data.Map);
            mapToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetMap(isOn);
                if (inGame)
                {
                    MinimapManager.Instance.SetVisibility(isOn);
                }
            });

            // Footsteps
            footstepsToggle.SetIsOnWithoutNotify(SettingsManager.Data.Footsteps);
            footstepsToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetFootsteps(isOn);
            });

            // Reset Progress
            resetProgressButton.onClick.AddListener(delegate
            {
                ConfirmationDialog.Confirm(LocalizationUtility.Localize("settings_reset-progress_title"), LocalizationUtility.Localize("settings_reset-progress_message"), onYes: ResetProgress);
            });
            #endregion

            #region Controls
            // Sensitivity (Horizontal)
            sensitivityHorizontalSlider.value = SettingsManager.Data.SensitivityHorizontal;
            sensitivityHorizontalSlider.onValueChanged.AddListener(delegate (float value)
            {
                SettingsManager.Instance.SetSensitivityHorizontal(value, inGame);
            });

            // Sensitivity (Vertical)
            sensitivityVerticalSlider.value = SettingsManager.Data.SensitivityVertical;
            sensitivityVerticalSlider.onValueChanged.AddListener(delegate (float value)
            {
                SettingsManager.Instance.SetSensitivityVertical(value, inGame);
            });

            // Invert Horizontal
            invertHorizontalToggle.SetIsOnWithoutNotify(SettingsManager.Data.InvertHorizontal);
            invertHorizontalToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetInvertHorizontal(isOn, inGame);
            });

            // Invert Vertical
            invertVerticalToggle.SetIsOnWithoutNotify(SettingsManager.Data.InvertVertical);
            invertVerticalToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetInvertVertical(isOn, inGame);
            });

            // Flip Button
            flipButtonToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetFlipButton(isOn);
            });
            flipButtonToggle.isOn = SettingsManager.Data.FlipButton;

            // Zoom Buttons
            zoomButtonsToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                SettingsManager.Instance.SetZoomButtons(isOn);
            });
            zoomButtonsToggle.isOn = SettingsManager.Data.ZoomButtons;

            if (SystemUtility.IsDevice(DeviceType.Handheld))
            {
                // Scale
                interfaceScaleSlider.onValueChanged.AddListener(delegate (float value)
                {
                    if (inGame)
                    {
                        foreach (PlatformSpecificScaler scaler in MobileControlsManager.Instance.MobileControlsUI.Scalers)
                        {
                            scaler.SetScale(scaler.Scale * value);
                        }
                    }

                    SettingsManager.Data.InterfaceScale = value;
                });
                interfaceScaleSlider.value = SettingsManager.Data.InterfaceScale;

                // Joystick
                joystickOS.SetupUsingEnum<Settings.JoystickType>();
                joystickOS.OnSelected.AddListener(delegate (int option)
                {
                    Settings.JoystickType type = (Settings.JoystickType)option;

                    if (inGame)
                    {
                        MobileControlsManager.Instance.MobileControlsUI.FixedJoystick.gameObject.SetActive(type == Settings.JoystickType.Fixed);
                        MobileControlsManager.Instance.MobileControlsUI.FloatJoystick.gameObject.SetActive(type == Settings.JoystickType.Floating);
                    }

                    bool show = type == Settings.JoystickType.Fixed;
                    joystickHorizontalCG.interactable = joystickVerticalCG.interactable = show;
                    joystickHorizontalCG.alpha = joystickVerticalCG.alpha = show ? 1f : 0.25f;

                    SettingsManager.Instance.SetJoystick(type);
                });
                joystickOS.Select(SettingsManager.Data.Joystick);

                // Joystick Position (Horizontal)
                joystickHorizontalSlider.onValueChanged.AddListener(delegate (float value)
                {
                    if (inGame)
                    {
                        RectTransform rt = MobileControlsManager.Instance.MobileControlsUI.FixedJoystick.transform as RectTransform;
                        rt.anchoredPosition = new Vector2(value * Screen.width, rt.anchoredPosition.y);
                    }

                    SettingsManager.Instance.SetJoystickPositionHorizontal(value);
                });
                joystickHorizontalSlider.value = SettingsManager.Data.JoystickPositionHorizontal;

                // Joystick Position (Vertical)
                joystickVerticalSlider.onValueChanged.AddListener(delegate (float value)
                {
                    if (inGame)
                    {
                        RectTransform rt = MobileControlsManager.Instance.MobileControlsUI.FixedJoystick.transform as RectTransform;
                        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, value * Screen.height);
                    }

                    SettingsManager.Instance.SetJoystickPositionVertical(value);
                });
                joystickVerticalSlider.value = SettingsManager.Data.JoystickPositionVertical;
            }
            #endregion

            Application.lowMemory += OnLowMemory;
        }
        private void Shutdown()
        {
            SettingsManager.Instance?.Save();
            Application.lowMemory -= OnLowMemory;
        }

        #region Video
        public void ApplyResolution()
        {
            SettingsManager.Instance.SetResolution(Screen.resolutions[resolutionOS.SelectedIndex]);
        }
        public void AppleScreenScale()
        {
            SettingsManager.Instance.SetScreenScale(screenScaleSlider.value);
        }

        private void OnLowMemory()
        {
            Resources.UnloadUnusedAssets();

            //InformationDialog.Inform(LocalizationUtility.Localize("low-memory_title"), LocalizationUtility.Localize("low-memory_message"));

            Application.lowMemory -= OnLowMemory;
        }
        #endregion

        #region Audio
        private IEnumerator PreviewMusicRoutine(string music)
        {
            MusicManager.Instance.FadeTo(music);
            yield return new WaitForSeconds(5f);
            MusicManager.Instance.FadeTo(null);
        }
        #endregion

        #region Gameplay
        public void RestorePurchases()
        {
            PremiumManager.Instance.Extensions.GetExtension<IAppleExtensions>().RestoreTransactions(delegate (bool isRestored, string message) 
            {
                if (isRestored)
                {
                    InformationDialog.Inform(LocalizationUtility.Localize("settings_gameplay_purchases-restored_title"), LocalizationUtility.Localize("settings_gameplay_purchases-restored_message"));
                }
            });
        }
        public void ViewPremium()
        {
            PremiumMenu.Instance.RequestNothing();
        }

        public void ViewUnlockableBodyParts()
        {
            UnlockableBodyPartsMenu.Instance.Open();
        }
        public void ViewUnlockablePatterns()
        {
            UnlockablePatternsMenu.Instance.Open();
        }
        public void ViewAchievements()
        {
            if (SystemUtility.IsDevice(DeviceType.Handheld) && GameServices.Instance.IsLoggedIn())
            {
                GameServices.Instance.ShowAchievementsUI();
            }
            else
            {
                AchievementsMenu.Instance.Open();
            }
        }
        public void ViewSourceCode()
        {
            Application.OpenURL("https://github.com/daniellochner/creature-creator");

            StatsManager.Instance.UnlockAchievement("ACH_HACKERMAN");
        }
        public void ChooseLanguage()
        {
            LocalizationMenu.Instance.Open();
        }

        public void ResetProgress()
        {
            ProgressManager.Instance.Revert();

            StatsManager.Instance.Revert();

            ProgressUI.Instance.UpdateInfo();
            UnlockableBodyPartsMenu.Instance.UpdateInfo();
            UnlockablePatternsMenu.Instance.UpdateInfo();
        }

        private void SelectFiles(string[] files)
        {
            SettingsManager.Data.CreaturePresets.Clear();
            foreach (string path in files)
            {
                SecretKey key = DatabaseManager.GetDatabaseEntry<SecretKey>("Keys", "Creature");
                CreatureData creature = SaveUtility.Load<CreatureData>(path, key.Value);
                if (creature != null)
                {
                    SettingsManager.Data.CreaturePresets.Add(creature);
                }
            }
            SettingsManager.Instance.Save();

            UpdatePresetSelectionUI();
        }
        private void OnOpenFilesComplete(bool selected, string singleFile, string[] files)
        {
            if (selected)
            {
                SelectFiles(files);
            }
        }
        private void UpdatePresetSelectionUI()
        {
            int presets = SettingsManager.Data.CreaturePresets.Count;
            clearFilesGO.SetActive(presets > 0);
            selectFilesGO.SetActive(presets <= 0);
            creaturePresetsText.text = presets.ToString();
        }
        #endregion
        #endregion
    }
}