// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DanielLochner.Assets.CreatureCreator
{
    public class PauseMenu : Dialog<PauseMenu>
    {
        #region Properties
        private bool CanToggle => !CinematicManager.Instance.IsInCinematic && !ConfirmationDialog.Instance.IsOpen && !InformationDialog.Instance.IsOpen && !InputDialog.Instance.IsOpen && !UnlockableBodyPartsMenu.Instance.IsOpen && !UnlockablePatternsMenu.Instance.IsOpen && !KeybindingsDialog.Instance.IsOpen;
        #endregion

        #region Methods
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && CanToggle)
            {
                Toggle();
            }
        }
        protected override void LateUpdate()
        {
            /* Override default close-on-ESC behaviour */
        }

        public void Leave()
        {
            if (CustomMapLoader.IsCustomMapLoadedFromSDK)
            {
                ConfirmationDialog.Confirm(LocalizationUtility.Localize("mainmenu_quit_title"), LocalizationUtility.Localize("mainmenu_quit_message"), onYes: delegate
                {
                    Application.Quit();
                });
            }
            else
            {
                ConfirmationDialog.Confirm(LocalizationUtility.Localize("leave_title"), LocalizationUtility.Localize("leave_message"), onYes: delegate
                {
                    if (WorldManager.Instance.IsMultiplayer)
                    {
                        NetworkConnectionManager.Instance.Leave();
                    }
                    else
                    {
                        NetworkShutdownManager.Instance.Shutdown();
                        SceneManager.LoadScene("MainMenu");
                    }
                    MusicManager.Instance.FadeTo(null, 0f, 1f);
                });
            }
        }

        public override void Open(bool instant = false)
        {
            base.Open(instant);
            PremiumManager.Instance.ShowBannerAd();
        }
        public override void Close(bool instant = false)
        {
            base.Close(instant);
            PremiumManager.Instance.HideBannerAd();
        }
        #endregion
    }
}