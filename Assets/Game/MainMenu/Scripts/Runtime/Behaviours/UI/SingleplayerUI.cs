// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace DanielLochner.Assets.CreatureCreator
{
    public class SingleplayerUI : MonoBehaviour
    {
        #region Fields
        [SerializeField] private Menu singleplayerMenu;
        [SerializeField] private OptionSelector mapOS;
        [SerializeField] private OptionSelector modeOS;
        [SerializeField] private OptionSelector customMapOS;
        [SerializeField] private Toggle npcToggle;
        [SerializeField] private Toggle pveToggle;
        [SerializeField] private CanvasGroup pveCG;
        [SerializeField] private Toggle unlimitedToggle;
        [SerializeField] private CanvasGroup unlimitedCG;
        [SerializeField] private CanvasGroup customIdsCG;
        [SerializeField] private GameObject customIdsGO;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private BlinkingText statusBT;
        [SerializeField] private MapUI mapUI;
        [SerializeField] private bool showComingSoon;

        private Coroutine updateStatusCoroutine;
        #endregion

        #region Methods
        private void Start()
        {
            Setup();
        }

        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnFailed;
        }
        private void OnDisable()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnFailed;
            }
            statusText.text = "";
        }

        public void Setup()
        {
            var customMapPaths = Directory.GetDirectories(CCConstants.MapsDir);
            var ignoredMaps = new List<Map>()
            {
                Map.ComingSoon
            };
            var hasCustom = customMapPaths.Length > 0;
            if (!hasCustom)
            {
                ignoredMaps.Add(Map.Custom);
            }
            mapOS.SetupUsingEnum<Map>(ignoredMaps.ToArray());
            mapOS.Select(Map.Island, false);
            mapOS.OnSelected.AddListener(delegate (int option)
            {
                bool showCustomIds = (Map)option == Map.Custom;
                customIdsCG.interactable = showCustomIds;
                customIdsCG.alpha = showCustomIds ? 1f : 0.25f;
            });
            singleplayerMenu.OnOpen += UpdateMap;

            customIdsGO.SetActive(hasCustom);
            if (hasCustom)
            {
                foreach (var customMapPath in customMapPaths)
                {
                    MapConfigData config = SaveUtility.Load<MapConfigData>(Path.Combine(customMapPath, "config.json"));

                    string customMapId = Path.GetFileNameWithoutExtension(customMapPath);
                    string customMapName = config.Name;

                    customMapOS.Options.Add(new CustomMapOption()
                    {
                        Id = $"{customMapId}#{customMapName}",
                    });
                }
                customMapOS.Select(0, false);
            }

            modeOS.SetupUsingEnum<Mode>();
            modeOS.OnSelected.AddListener(delegate (int option)
            {
                bool showUnlimited = option == 1;
                unlimitedCG.interactable = showUnlimited;
                unlimitedCG.alpha = showUnlimited ? 1f : 0.25f;
            });
            modeOS.Select(Mode.Adventure, false);

            npcToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                pveCG.interactable = isOn;
                pveCG.alpha = isOn ? 1f : 0.25f;
            });
        }

        public void Play()
        {
            try
            {
                // Setup World
                Map map = (Map)mapOS.SelectedIndex;
                Mode mode = (Mode)modeOS.SelectedIndex;
                bool spawnNPC = npcToggle.isOn;
                bool enablePVE = pveToggle.isOn;
                bool unlimited = unlimitedToggle.isOn && (mode == Mode.Creative);

                string customMapId = "";
                if (map == Map.Custom)
                {
                    CustomMapOption customMapOption = (CustomMapOption)customMapOS.Selected;
                    customMapId = customMapOption.MapId;
                }

                WorldManager.Instance.World = new WorldSP(map, mode, spawnNPC, enablePVE, unlimited, customMapId);

                // Check Premium
                if (unlimited && !PremiumManager.Data.IsPremium)
                {
                    throw new Exception(LocalizationUtility.Localize("mainmenu_premium_unlimited"));
                }

                // Check Map
                if (map == Map.ComingSoon)
                {
                    throw new Exception(LocalizationUtility.Localize("mainmenu_map-coming-soon"));
                }
                if ((mode == Mode.Adventure) && !ProgressManager.Instance.IsMapUnlocked(map))
                {
                    throw new Exception(LocalizationUtility.Localize("mainmenu_map-locked", LocalizationUtility.Localize($"option_map_{map}".ToLower())));
                }

                // Set Connection Data
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = NetworkTransportPicker.Instance.GetTransport<UnityTransport>("localhost");
                NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new ConnectionData("", "", "", ProgressManager.Data.Level)));

                // Start Host
                NetworkManager.Singleton.StartHost();
            }
            catch (Exception e)
            {
                UpdateStatus(e.Message, Color.red);
            }
        }

        private void OnFailed(ulong clientId)
        {
            UpdateStatus("Failed to create world...", Color.red);
        }

        private void UpdateMap()
        {
            mapUI.Setup(mapOS, modeOS, customMapOS);
        }
        private void UpdateStatus(string status, Color color, float duration = 5)
        {
            if (updateStatusCoroutine != null)
            {
                StopCoroutine(updateStatusCoroutine);
            }

            statusText.CrossFadeAlpha(0f, 0f, true);
            statusText.CrossFadeAlpha(1f, 0.25f, true);
            statusText.text = status;
            statusText.color = color;
            statusBT.IsBlinking = false;

            if (duration == -1)
            {
                statusBT.IsBlinking = true;
            }
            else
            {
                updateStatusCoroutine = this.Invoke(HideStatus, duration);
            }
        }
        private void HideStatus()
        {
            statusText.CrossFadeAlpha(0, 0.25f, true);
        }
        #endregion
    }
}