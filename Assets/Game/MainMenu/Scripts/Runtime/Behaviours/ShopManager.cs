// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System.Collections;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class ShopManager : MonoBehaviourSingleton<ShopManager>
    {
        #region Fields
        [SerializeField] private GameObject shopButton;
        #endregion

        #region Properties
        public int ShownAttempts
        {
            get => PlayerPrefs.GetInt("SHOWN_ATTEMPTS", 0);
            set => PlayerPrefs.SetInt("SHOWN_ATTEMPTS", value);
        }
        #endregion

        #region Methods
        private void Start()
        {
            shopButton.SetActive(!SettingsManager.Instance.ShowTutorial && ShownAttempts > 5);
        }

        public IEnumerator Setup()
        {
            if (ShownAttempts % 10 == 1)
            {
                ShopMenu.Instance.Open();

                yield return new WaitUntil(() => !ShopMenu.Instance.IsOpen);
            }

            ShownAttempts++;
        }
        #endregion
    }
}