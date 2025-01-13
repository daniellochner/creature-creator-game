// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using IngameDebugConsole;
using MoreMountains.NiceVibrations;
using Pinwheel.Griffin;
using Pinwheel.Jupiter;
using Pinwheel.Poseidon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering.PostProcessing;
using static DanielLochner.Assets.CreatureCreator.Settings;

namespace DanielLochner.Assets.CreatureCreator
{
    public class SettingsManager : DataManager<SettingsManager, Settings>
    {
        #region Fields
        [Header("Settings")]
        [SerializeField] private CreatureQuality creatureQualitySettings;
        [SerializeField] private GameObject[] cameras;
        [SerializeField] private PostProcessProfile[] profiles;
        [SerializeField] private PWaterProfile[] waterProfiles;
        [SerializeField] private GRendering[] renderingProfiles;
        [SerializeField] private JSkyProfile[] skyProfiles;
        [SerializeField] private Material[] windMaterials;
        [Space]
        [SerializeField] private AudioMixer masterAudioMixer;
        [SerializeField] private MinMax minMaxVolumeDB = new MinMax(-20, 1);
        [Space]
        [SerializeField] private NetworkStatsManager statsManagerPrefab;
        [SerializeField] private GameObject creatureBasePrefab;
        [SerializeField] private GameObject creaturePlayerPrefab;
        [Space]
        [SerializeField] private CameraOrbit cameraOrbitPrefab;
        #endregion

        #region Properties
        public bool ShowTutorial => Data.Tutorial && (ProgressManager.Data.UnlockedBodyParts.Count == 0 && ProgressManager.Data.UnlockedPatterns.Count == 0) && (!EditorManager.Instance || !EditorManager.Instance.CreativeMode);
        #endregion

        #region Methods
        protected override void Start()
        {
            base.Start();

            SetResolution(Data.Resolution);
            SetFullscreen(Data.Fullscreen);
            SetVSync(Data.VSync);
            SetTargetFrameRate(Data.TargetFrameRate);

            SetCreatureMeshQuality(Data.CreatureMeshQuality);
            SetShadowQuality(Data.ShadowQuality);
            SetTextureQuality(Data.TextureQuality);
            SetAmbientOcclusion(Data.AmbientOcclusion);
            SetAntialiasing(Data.Antialiasing);
            SetScreenSpaceReflections(Data.ScreenSpaceReflections);
            SetFoliage(Data.Foliage);
            SetAmbientParticles(Data.AmbientParticles);
            SetReflections(Data.Reflections);
            SetAnisotropicFiltering(Data.AnisotropicFiltering);
            SetBloom(Data.Bloom);
            SetDebugMode(Data.DebugMode);
            SetDepthOfField(Data.DepthOfField);
            SetMotionBlur(Data.MotionBlur);
            SetOptimizeCreatures(Data.OptimizeCreatures);
            SetOptimizeLighting(Data.OptimizeLighting);
            SetCreatureRenderDistance(Data.CreatureRenderDistance);

            SetMasterVolume(Data.MasterVolume);
            SetMusicVolume(Data.MusicVolume);
            SetSoundEffectsVolume(Data.SoundEffectsVolume);
            SetInGameMusic(Data.InGameMusic);

            SetOnlineUsername(Data.OnlineUsername);
            SetExportPrecision(Data.ExportPrecision);
            SetTouchOffset(Data.TouchOffset);
            SetExportAll(Data.ExportAll);
            SetCameraShake(Data.CameraShake);
            SetDebugMode(Data.DebugMode);
            SetPreviewFeatures(Data.PreviewFeatures);
            SetNetworkStats(Data.NetworkStats);
            SetTutorial(Data.Tutorial);
            SetWorldChat(Data.WorldChat);
            SetFootsteps(Data.Footsteps);
            SetMap(Data.Map);

            SetSensitivityHorizontal(Data.SensitivityHorizontal);
            SetSensitivityVertical(Data.SensitivityVertical);
            SetInvertHorizontal(Data.InvertHorizontal);
            SetInvertVertical(Data.InvertVertical);
            SetFlipButton(Data.FlipButton);
            SetZoomButtons(Data.ZoomButtons);

            if (SystemUtility.IsDevice(DeviceType.Handheld))
            {
                SetScreenScale(Data.ScreenScale); // Only override screen scale on mobile!
                OptimizeForMobile();
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Save();
        }

        #region Video
        public void SetResolution(Resolution resolution)
        {
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen, resolution.refreshRate);
            Data.Resolution = resolution;
        }
        public void SetFullscreen(bool fullscreen)
        {
            Screen.fullScreen = Data.Fullscreen = fullscreen;
        }
        public void SetVSync(bool vSync)
        {
            QualitySettings.vSyncCount = vSync ? 1 : 0;
            Data.VSync = vSync;
        }
        public void SetScreenScale(float screenScale)
        {
            int width = Mathf.FloorToInt(Display.main.systemWidth * screenScale);
            int height = Mathf.FloorToInt(Display.main.systemHeight * screenScale);
            int refreshRate = Data.Resolution.refreshRate;

            Resolution targetResolution = new Resolution()
            {
                width = width,
                height = height,
                refreshRate = refreshRate
            };
            SetResolution(targetResolution);

            Data.ScreenScale = screenScale;
        }
        public void SetTargetFrameRate(int targetFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
            Data.TargetFrameRate = targetFrameRate;
        }

        public void SetCreatureMeshQuality(CreatureMeshQualityType type)
        {
            switch (type)
            {
                case CreatureMeshQualityType.Low:
                    creatureQualitySettings.Segments = 8;
                    creatureQualitySettings.Rings = 2;
                    break;
                case CreatureMeshQualityType.Medium:
                    creatureQualitySettings.Segments = 16;
                    creatureQualitySettings.Rings = 4;
                    break;
                case CreatureMeshQualityType.High:
                    creatureQualitySettings.Segments = 20;
                    creatureQualitySettings.Rings = 8;
                    break;
            }
            Data.CreatureMeshQuality = type;
        }
        public void SetShadowQuality(ShadowQualityType type)
        {
            if (type == ShadowQualityType.None)
            {
                QualitySettings.shadows = ShadowQuality.Disable;
            }
            else
            {
                QualitySettings.shadows = ShadowQuality.All;
                switch (type)
                {
                    case ShadowQualityType.Low:
                        QualitySettings.shadowResolution = ShadowResolution.Low;
                        QualitySettings.shadowDistance = 50;
                        break;
                    case ShadowQualityType.Medium:
                        QualitySettings.shadowResolution = ShadowResolution.Medium;
                        QualitySettings.shadowDistance = 50;
                        break;
                    case ShadowQualityType.High:
                        QualitySettings.shadowResolution = ShadowResolution.High;
                        QualitySettings.shadowDistance = 100;
                        break;
                    case ShadowQualityType.VeryHigh:
                        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                        QualitySettings.shadowDistance = 100;
                        break;
                }
            }
            Data.ShadowQuality = type;
        }
        public void SetTextureQuality(TextureQualityType type)
        {
            switch (type)
            {
                case TextureQualityType.VeryLow:
                    QualitySettings.masterTextureLimit = 4;
                    break;
                case TextureQualityType.Low:
                    QualitySettings.masterTextureLimit = 3;
                    break;
                case TextureQualityType.Medium:
                    QualitySettings.masterTextureLimit = 2;
                    break;
                case TextureQualityType.High:
                    QualitySettings.masterTextureLimit = 1;
                    break;
                case TextureQualityType.VeryHigh:
                    QualitySettings.masterTextureLimit = 0;
                    break;
            }
            Data.TextureQuality = type;
        }
        public void SetAmbientOcclusion(AmbientOcclusionType type)
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer && type == AmbientOcclusionType.MSVO) // TODO: Quick fix for AO issues on Mac and Linux...
            {
                type = AmbientOcclusionType.SAO;
            }

            foreach (PostProcessProfile profile in profiles)
            {
                if (profile.TryGetSettings(out AmbientOcclusion ao))
                {
                    if (type == AmbientOcclusionType.None)
                    {
                        ao.active = false;
                    }
                    else
                    {
                        ao.active = true;
                        switch (type)
                        {
                            case AmbientOcclusionType.SAO:
                                ao.mode = new AmbientOcclusionModeParameter() { value = AmbientOcclusionMode.ScalableAmbientObscurance };
                                break;
                            case AmbientOcclusionType.MSVO:
                                ao.mode = new AmbientOcclusionModeParameter() { value = AmbientOcclusionMode.MultiScaleVolumetricObscurance };
                                break;
                        }
                    }
                }
            }
            Data.AmbientOcclusion = type;
        }
        public void SetAntialiasing(AntialiasingType type, bool updateMain = false)
        {
            List<PostProcessLayer> layers = new List<PostProcessLayer>();
            foreach (GameObject camera in cameras)
            {
                layers.AddRange(camera.GetComponentsInChildren<PostProcessLayer>(true));
            }
            if (updateMain)
            {
                layers.AddRange(Camera.main.GetComponents<PostProcessLayer>());
            }

            foreach (PostProcessLayer layer in layers)
            {
                switch (type)
                {
                    case AntialiasingType.None:
                        layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                        break;
                    case AntialiasingType.FXAA:
                        layer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                        break;
                    case AntialiasingType.LowSMAA:
                        layer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                        layer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Low;
                        break;
                    case AntialiasingType.MediumSMAA:
                        layer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                        layer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Medium;
                        break;
                    case AntialiasingType.HighSMAA:
                        layer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                        layer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.High;
                        break;
                    case AntialiasingType.Temporal:
                        layer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                        break;
                }
            }
            Data.Antialiasing = type;
        }
        public void SetScreenSpaceReflections(ScreenSpaceReflectionsType type)
        {
            foreach (PostProcessProfile profile in profiles)
            {
                if (profile.TryGetSettings(out ScreenSpaceReflections ssr))
                {
                    if (type == ScreenSpaceReflectionsType.None)
                    {
                        ssr.active = false;
                    }
                    else
                    {
                        ssr.active = true;
                        switch (type)
                        {
                            case ScreenSpaceReflectionsType.Low:
                                ssr.preset = new ScreenSpaceReflectionPresetParameter() { value = ScreenSpaceReflectionPreset.Low };
                                break;
                            case ScreenSpaceReflectionsType.Medium:
                                ssr.preset = new ScreenSpaceReflectionPresetParameter() { value = ScreenSpaceReflectionPreset.Medium };
                                break;
                            case ScreenSpaceReflectionsType.High:
                                ssr.preset = new ScreenSpaceReflectionPresetParameter() { value = ScreenSpaceReflectionPreset.High };
                                break;
                            case ScreenSpaceReflectionsType.VeryHigh:
                                ssr.preset = new ScreenSpaceReflectionPresetParameter() { value = ScreenSpaceReflectionPreset.Higher };
                                break;
                        }
                    }
                }
            }
            Data.ScreenSpaceReflections = type;
        }
        public void SetFoliage(FoliageType type)
        {
            foreach (GRendering terrainProfile in renderingProfiles)
            {
                switch (type)
                {
                    case FoliageType.VeryLow:
                        terrainProfile.DrawGrasses = false;
                        terrainProfile.TreeDistance = 100;
                        foreach (Material material in windMaterials)
                        {
                            material.shader = Shader.Find("Mobile/Diffuse");
                        }
                        break;
                    case FoliageType.Low:
                        terrainProfile.DrawGrasses = true;
                        terrainProfile.GrassDistance = 50;
                        terrainProfile.TreeDistance = 150;
                        foreach (Material material in windMaterials)
                        {
                            material.shader = Shader.Find("Mobile/Diffuse");
                        }
                        break;
                    case FoliageType.Medium:
                        terrainProfile.DrawGrasses = true;
                        terrainProfile.GrassDistance = 100;
                        terrainProfile.TreeDistance = 200;
                        foreach (Material material in windMaterials)
                        {
                            material.shader = Shader.Find("Nicrom/LPW/ASE/Low Poly Vegetation");
                        }
                        break;
                    case FoliageType.High:
                        terrainProfile.DrawGrasses = true;
                        terrainProfile.GrassDistance = 150;
                        terrainProfile.TreeDistance = 250;
                        foreach (Material material in windMaterials)
                        {
                            material.shader = Shader.Find("Nicrom/LPW/ASE/Low Poly Vegetation");
                        }
                        break;
                    case FoliageType.VeryHigh:
                        terrainProfile.DrawGrasses = true;
                        terrainProfile.GrassDistance = 200;
                        terrainProfile.TreeDistance = 300;
                        foreach (Material material in windMaterials)
                        {
                            material.shader = Shader.Find("Nicrom/LPW/ASE/Low Poly Vegetation");
                        }
                        break;
                }
            }
            Data.Foliage = type;
        }
        public void SetAmbientParticles(bool ambientParticles)
        {
            foreach (JSkyProfile profile in skyProfiles)
            {
                profile.EnableOverheadCloud = ambientParticles;
            }
            Data.AmbientParticles = ambientParticles;
        }
        public void SetReflections(bool reflections)
        {
            //foreach (PWaterProfile waterProfile in waterProfiles)
            //{
            //    waterProfile.EnableReflection = reflections;
            //}
            Data.Reflections = reflections;
        }
        public void SetAnisotropicFiltering(bool anisotropicFiltering)
        {
            QualitySettings.anisotropicFiltering = anisotropicFiltering ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
            Data.AnisotropicFiltering = anisotropicFiltering;
        }
        public void SetBloom(bool bloom)
        {
            foreach (PostProcessProfile profile in profiles)
            {
                if (profile.TryGetSettings(out Bloom b))
                {
                    b.active = bloom;
                }
            }
            Data.Bloom = bloom;
        }
        public void SetDepthOfField(bool depthOfField)
        {
            foreach (PostProcessProfile profile in profiles)
            {
                if (profile.TryGetSettings(out DepthOfField dof))
                {
                    dof.active = depthOfField;
                }
            }
            Data.DepthOfField = depthOfField;
        }
        public void SetMotionBlur(bool motionBlur)
        {
            foreach (PostProcessProfile profile in profiles)
            {
                if (profile.TryGetSettings(out MotionBlur mb))
                {
                    mb.active = motionBlur;
                }
            }
            Data.MotionBlur = motionBlur;
        }
        public void SetOptimizeCreatures(bool optimizeCreatures)
        {
            Data.OptimizeCreatures = optimizeCreatures;
        }
        public void SetOptimizeLighting(bool optimizeLighting)
        {
            Data.OptimizeLighting = optimizeLighting;
        }
        public void SetCreatureRenderDistance(float creatureRenderDistance)
        {
            Data.CreatureRenderDistance = creatureRenderDistance;
        }

        private void OptimizeForMobile()
        {
            Time.fixedDeltaTime = 0.025f;

            foreach (PostProcessProfile profile in profiles)
            {
                if (profile.TryGetSettings(out Blur blur))
                {
                    blur.active = false;
                }
                if (profile.TryGetSettings(out Bloom bloom))
                {
                    bloom.fastMode.value = true;
                }
            }

            foreach (GRendering renderingProfile in renderingProfiles)
            {
                foreach (GGrassPrototype grassPrototype in renderingProfile.TerrainData.Foliage.Grasses.Prototypes)
                {
                    grassPrototype.ShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

            SetTargetFrameRate(60);
        }
        #endregion

        #region Audio
        public void SetMasterVolume(float t)
        {
            Data.MasterVolume = t;
            SetMusicVolume(Data.MusicVolume);
            SetSoundEffectsVolume(Data.SoundEffectsVolume);
        }
        public void SetMusicVolume(float t)
        {
            Data.MusicVolume = t;
            SetVolume("MusicVolume", t);

            foreach (AudioSource source in MusicManager.Instance.Sources)
            {
                if (t > 0)
                {
                    source.UnPause();
                }
                else
                {
                    source.Pause();
                }
            }
        }
        public void SetSoundEffectsVolume(float t)
        {
            Data.SoundEffectsVolume = t;
            SetVolume("SoundEffectsVolume", t);

            if (AmbienceManager.Instance != null)
            {
                foreach (AudioSource source in AmbienceManager.Instance.Sources)
                {
                    if (t > 0)
                    {
                        source.UnPause();
                    }
                    else
                    {
                        source.Pause();
                    }
                }
            }
        }
        private void SetVolume(string param, float t)
        {
            t *= Data.MasterVolume;

            float volume = 0f;
            if (t == 0f)
            {
                volume = -80f;
            }
            else
            {
                volume = Mathf.Lerp(minMaxVolumeDB.min, minMaxVolumeDB.max, t);
            }
            masterAudioMixer.SetFloat(param, volume);
        }
        private float GetVolume(string param)
        {
            if (masterAudioMixer.GetFloat("param", out float value))
            {
                return Mathf.InverseLerp(minMaxVolumeDB.min, minMaxVolumeDB.max, value);
            }
            return 0f;
        }
        public void SetInGameMusic(InGameMusicType type)
        {
            Data.InGameMusic = type;
        }
        #endregion

        #region Gameplay
        public void SetOnlineUsername(string username)
        {
            Data.OnlineUsername = username;
        }
        public void SetExportPrecision(int precision)
        {
            Data.ExportPrecision = precision;
        }
        public void SetTouchOffset(float touchOffset)
        {
            CreatureEditor editor = creaturePlayerPrefab.GetComponent<CreatureEditor>();
            editor.TouchOffset = Data.TouchOffset = touchOffset;
        }
        public void SetExportAll(bool exportAll)
        {
            Data.ExportAll = exportAll;
        }
        public void SetCameraShake(bool cameraShake, bool updateMain = false)
        {
            List<StressReceiver> receivers = new List<StressReceiver>();
            foreach (GameObject camera in cameras)
            {
                receivers.AddRange(camera.GetComponents<StressReceiver>());
            }
            if (updateMain)
            {
                receivers.AddRange(Camera.main.GetComponents<StressReceiver>());
            }

            foreach (StressReceiver receiver in receivers)
            {
                receiver.enabled = cameraShake;
                receiver.Reset();
            }
            Data.CameraShake = cameraShake;
        }
        public void SetVibrations(bool vibrations)
        {
            MMVibrationManager.SetHapticsActive(vibrations);
            Data.Vibrations = vibrations;
        }
        public void SetDebugMode(bool debugMode)
        {
            DebugLogManager.Instance.gameObject.SetActive(debugMode);
            Data.DebugMode = debugMode;
        }
        public void SetPreviewFeatures(bool previewFeatures)
        {
            Data.PreviewFeatures = previewFeatures;
        }
        public void SetNetworkStats(bool networkStats)
        {
            statsManagerPrefab.UseStats = networkStats;
            Data.NetworkStats = networkStats;
        }
        public void SetTutorial(bool tutorial)
        {
            Data.Tutorial = tutorial;
        }
        public void SetWorldChat(bool worldChat)
        {
            Data.WorldChat = worldChat;
        }
        public void SetMap(bool map)
        {
            Data.Map = map;
        }
        public void SetFootsteps(bool footsteps)
        {
            Data.Footsteps = footsteps;
        }
        public void SetLocale(string locale)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(locale);
            Data.Locale = locale;
        }
        #endregion

        #region Controls
        public void SetSensitivityHorizontal(float sensitivity, bool updateMain = false)
        {
            Vector2 mouseSensitivity = new Vector2(sensitivity, Data.SensitivityVertical);

            cameraOrbitPrefab.MouseSensitivity = mouseSensitivity;
            if (updateMain)
            {
                Camera.main.GetComponentInParent<CameraOrbit>().MouseSensitivity = mouseSensitivity;
            }

            Data.SensitivityHorizontal = sensitivity;
        }
        public void SetSensitivityVertical(float sensitivity, bool updateMain = false)
        {
            Vector2 mouseSensitivity = new Vector2(Data.SensitivityHorizontal, sensitivity);

            cameraOrbitPrefab.MouseSensitivity = mouseSensitivity;
            if (updateMain)
            {
                Camera.main.GetComponentInParent<CameraOrbit>().MouseSensitivity = mouseSensitivity;
            }

            Data.SensitivityVertical = sensitivity;
        }
        public void SetInvertHorizontal(bool invert, bool updateMain = false)
        {
            cameraOrbitPrefab.InvertMouseX = invert;
            if (updateMain)
            {
                Camera.main.GetComponentInParent<CameraOrbit>().InvertMouseX = invert;
            }

            Data.InvertHorizontal = invert;
        }
        public void SetInvertVertical(bool invert, bool updateMain = false)
        {
            cameraOrbitPrefab.InvertMouseY = invert;
            if (updateMain)
            {
                Camera.main.GetComponentInParent<CameraOrbit>().InvertMouseY = invert;
            }

            Data.InvertVertical = invert;
        }

        public void SetJoystick(Settings.JoystickType type)
        {
            Data.Joystick = type;
        }
        public void SetJoystickPositionHorizontal(float position)
        {
            Data.JoystickPositionHorizontal = position;
        }
        public void SetJoystickPositionVertical(float position)
        {
            Data.JoystickPositionVertical = position;
        }
        public void SetInterfaceScale(float scale)
        {
            Data.InterfaceScale = scale;
        }
        public void SetFlipButton(bool flipButton)
        {
            Data.FlipButton = flipButton;
        }
        public void SetZoomButtons(bool zoomButtons)
        {
            Data.ZoomButtons = zoomButtons;
        }
        #endregion
        #endregion
    }
}