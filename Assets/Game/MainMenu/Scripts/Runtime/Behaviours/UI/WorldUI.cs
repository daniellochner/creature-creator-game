using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace DanielLochner.Assets.CreatureCreator
{
    public class WorldUI : MonoBehaviour
    {
        #region Fields
        [SerializeField] private TextMeshProUGUI playersText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI mapText;
        [SerializeField] private GameObject padlockIcon;
        [SerializeField] private Button infoButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Color moddedColour;
        [SerializeField] private Color betaColour;
        #endregion

        #region Properties
        public Button JoinButton => joinButton;

        public int Players { get; private set; }
        #endregion

        #region Methods
        public void Setup(MultiplayerUI multiplayerUI, Lobby lobby)
        {
            Players = lobby.Players.Count;

            playersText.text = $"{Players}/{lobby.MaxPlayers}";

            WorldMP world = new WorldMP(lobby);
            mapText.text = $"{FormatMap(world.MapId)} ({FormatMode(world.Mode == Mode.Creative)})";
            joinButton.onClick.AddListener(delegate 
            {
                multiplayerUI.Join(world.Id);
            });

            string worldName = lobby.Name.NoParse();
            if (world.IsBeta)
            {
                worldName = "[BETA] ".ToColour(betaColour) + worldName;
            }
            if (world.IsModded)
            {
                worldName = "[MODDED] ".ToColour(moddedColour) + worldName;
            }
            nameText.text = worldName;

            infoButton.onClick.AddListener(delegate
            {
                InformationDialog.Inform(world.WorldName, 
                    $"<b>{LocalizationUtility.Localize("mainmenu_multiplayer_world_version")}:</b> {world.Version}<br>" +
                    $"<b>{LocalizationUtility.Localize("mainmenu_multiplayer_world_map")}:</b> {FormatMap(world.MapId)}<br>" + 
                    $"<b>{LocalizationUtility.Localize("mainmenu_multiplayer_world_mode")}:</b> {FormatMode(world.Mode == Mode.Creative)}<br>" +
                    $"<b>{LocalizationUtility.Localize("mainmenu_multiplayer_world_pvp")}:</b> {FormatEnabled(world.EnablePVP)}<br>" +
                    $"<b>{LocalizationUtility.Localize("mainmenu_multiplayer_world_npcs")}:</b> {FormatEnabled(world.SpawnNPC)}<br>" +
                    $"<b>{LocalizationUtility.Localize("mainmenu_multiplayer_world_pve")}:</b> {FormatEnabled(world.EnablePVE)}<br>" +
                    $"<b>{LocalizationUtility.Localize("mainmenu_multiplayer_world_profanity")}:</b> {FormatAllowed(world.AllowProfanity)}",

                    okay: LocalizationUtility.Localize("mainmenu_multiplayer_join"),
                    onOkay: joinButton.onClick.Invoke
                    );
            });

            padlockIcon.SetActive(world.IsPasswordProtected);
        }

        private string FormatMap(string mapId)
        {
            return LocalizationUtility.Localize(mapId);
        }
        private string FormatMode(bool isCreativeMode)
        {
            return isCreativeMode ? LocalizationUtility.Localize("mainmenu_multiplayer_world_creative") : LocalizationUtility.Localize("mainmenu_multiplayer_world_adventure");
        }
        private string FormatEnabled(bool isEnabled)
        {
            return isEnabled ? LocalizationUtility.Localize("mainmenu_multiplayer_world_enabled") : LocalizationUtility.Localize("mainmenu_multiplayer_world_disabled");
        }
        private string FormatAllowed(bool isAllowed)
        {
            return isAllowed ? LocalizationUtility.Localize("mainmenu_multiplayer_world_allowed") : LocalizationUtility.Localize("mainmenu_multiplayer_world_forbidden");
        }
        #endregion
    }
}