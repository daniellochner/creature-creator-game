// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System;
using System.Collections;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.Events;

namespace DanielLochner.Assets.CreatureCreator
{
    public class CountdownManager : MonoBehaviourSingleton<CountdownManager>
    {
        #region Fields
        [SerializeField] private CountdownUI countdownUI;
        [SerializeField] private UnityEvent onComplete;
        #endregion

        #region Methods
        public IEnumerator Setup(bool fetchConfig)
        {
            if (fetchConfig)
            {
                yield return RemoteConfigUtility.FetchConfigRoutine();
            }

            // Check Countdown
            CountdownData countdownData = JsonUtility.FromJson<CountdownData>(RemoteConfigService.Instance.appConfig.GetJson("countdown_data"));
            if (countdownData.date != -1)
            {
                yield return new WaitUntil(() => WorldTimeManager.Instance.IsInitialized);

                DateTime date = new DateTime(countdownData.date);
                if ((date - WorldTimeManager.Instance.UtcNow).TotalSeconds > 0)
                {
                    countdownUI.Setup(countdownData.title, date, onComplete.Invoke);
                }
                else
                {
                    onComplete.Invoke();
                }

                yield return new WaitForSeconds(1f);
            }
        }
        #endregion

        #region Nested
        public class CountdownData
        {
            public string title;
            public long date;
        }
        #endregion
    }
}