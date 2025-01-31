// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System.Collections;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class DailyRewardManager : MonoBehaviourSingleton<DailyRewardManager>
    {
        #region Properties
        public string DailyReward
        {
            get => PlayerPrefs.GetString("DAILY_REWARD");
            set => PlayerPrefs.SetString("DAILY_REWARD", value);
        }
        #endregion

        #region Methods
        public IEnumerator Setup()
        {
            // Initialize
            yield return new WaitUntil(() => WorldTimeManager.Instance.IsInitialized);

            // Check Daily Reward
            string today = WorldTimeManager.Instance.UtcNow.ToShortDateString();
            if (DailyReward != today)
            {
                DailyReward = today;

                PremiumManager.Data.DownloadsToday = 0;
                PremiumManager.Instance.Save();

                if (!PremiumManager.Instance.IsEverythingUsable())
                {
                    RewardsMenu.Instance.Clear();
                    PremiumManager.Instance.RequestedItem = null;
                    PremiumManager.Instance.AccessRandom(4);
                    RewardsMenu.Instance.Open(false, LocalizationUtility.Localize("daily-reward_title"));

                    yield return new WaitUntil(() => !RewardsMenu.Instance.IsOpen);
                }
            }
        }
        #endregion
    }
}