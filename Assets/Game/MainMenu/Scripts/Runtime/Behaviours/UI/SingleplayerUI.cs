// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System;
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
        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private OptionSelector mapOS;
        [SerializeField] private OptionSelector modeOS;
        [SerializeField] private Toggle npcToggle;
        [SerializeField] private Toggle pveToggle;
        [SerializeField] private Toggle unlimitedToggle;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private BlinkingText statusBT;

        private Coroutine updateStatusCoroutine;
        #endregion

        #region Properties
        private UnityTransport NetworkTransport => NetworkTransportPicker.Instance.GetTransport<UnityTransport>("localhost");
        #endregion

        #region Methods
        private void Start()
        {
            Setup();
        }
        private void OnDestroy()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnFailed;
            }
        }

        public void Setup()
        {
            mapOS.SetupUsingEnum<Map>();
            mapOS.Select(Map.Island, false);

            modeOS.SetupUsingEnum<Mode>();
            modeOS.OnSelected.AddListener(delegate (int option)
            {
                unlimitedToggle.transform.parent.parent.gameObject.SetActive(option == 1); // only show unlimited toggle for creative mode
            });
            modeOS.Select(Mode.Adventure);

            NetworkManager.Singleton.OnClientDisconnectCallback += OnFailed;
        }

        public void Play()
        {
            string mapName = ((Map)mapOS.Selected).ToString();
            bool creativeMode = ((Mode)modeOS.Selected) == Mode.Creative;
            bool spawnNPC = npcToggle.isOn;
            bool enablePVE = pveToggle.isOn;
            bool unlimited = unlimitedToggle.isOn && creativeMode;

            if (!ProgressManager.Instance.IsMapUnlocked(Enum.Parse<Map>(mapName)))
            {
                UpdateStatus(LocalizationUtility.Localize("mainmenu_map-locked", LocalizationUtility.Localize($"option_map_{mapName}".ToLower())), Color.white);
                return;
            }

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = NetworkTransport;
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new ConnectionData("", usernameInputField.text, "")));

            WorldManager.Instance.World = new WorldSP(mapName, creativeMode, spawnNPC, enablePVE, unlimited);

            NetworkManager.Singleton.StartHost();
        }

        private void OnFailed(ulong clientId)
        {
            UpdateStatus("Failed to create world...", Color.red);
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
                updateStatusCoroutine = this.Invoke(HideNetworkStatus, duration);
            }
        }
        private void HideNetworkStatus()
        {
            statusText.CrossFadeAlpha(0, 0.25f, true);
        }
        #endregion
    }
}