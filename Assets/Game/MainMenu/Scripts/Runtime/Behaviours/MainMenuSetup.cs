// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System.Collections;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class MainMenuSetup : MonoBehaviour
    {
        #region Methods
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);

            if (SettingsManager.Instance.ShowTutorial)
            {
                // Tutorial
                yield return MenuTutorialManager.Instance.Setup();
            }
            else
            {
                // Daily Reward
                if (!PremiumManager.Data.IsPremium)
                {
                    yield return DailyRewardManager.Instance.Setup();
                }
                
                // New Version
                yield return NewVersionManager.Instance.Setup(true);

                // Countdown
                yield return CountdownManager.Instance.Setup(false);

                // Promo
                yield return PromoManager.Instance.Setup(false);
            }
        }
        #endregion
    }
}