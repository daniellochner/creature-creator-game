using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DanielLochner.Assets
{
    public class CountdownUI : MonoBehaviour
    {
        #region Fields
        [SerializeField] private TextMeshProUGUI titleText;
        [Space]
        [SerializeField] private TextMeshProUGUI daysText;
        [SerializeField] private TextMeshProUGUI hoursText;
        [SerializeField] private TextMeshProUGUI minsText;
        [SerializeField] private TextMeshProUGUI secsText;
        #endregion

        #region Methods
        public void Setup(string title, DateTime date, Action onComplete)
        {
            gameObject.SetActive(true);
            titleText.text = title;
            StartCoroutine(CountdownRoutine(date, onComplete));
        }

        private IEnumerator CountdownRoutine(DateTime target, Action onComplete)
        {
            TimeSpan difference = TimeSpan.Zero;
            do
            {
                difference = target - WorldTimeManager.Instance.UtcNow;

                daysText.text = $"{difference.Days:00}";
                hoursText.text = $"{difference.Hours:00}";
                minsText.text = $"{difference.Minutes:00}";
                secsText.text = $"{difference.Seconds:00}";

                yield return new WaitForSeconds(1f);
            }
            while (difference.TotalSeconds > 0);

            onComplete?.Invoke();

            gameObject.SetActive(false);
        }
        #endregion
    }
}