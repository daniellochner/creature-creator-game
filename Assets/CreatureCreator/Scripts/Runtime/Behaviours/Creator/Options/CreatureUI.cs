// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DanielLochner.Assets.CreatureCreator
{
    public class CreatureUI : MonoBehaviour
    {
        #region Fields
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Toggle selectToggle;
        [SerializeField] private Button errorButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button shareButton;
        [SerializeField] private GameObject progress;
        #endregion

        #region Properties
        public TextMeshProUGUI NameText => nameText;
        public Toggle SelectToggle => selectToggle;
        public Button ErrorButton => errorButton;
        public Button RemoveButton => removeButton;
        public Button ShareButton => shareButton;
        public GameObject Progress => progress;
        #endregion

        #region Methods
        public void Setup(string creatureName)
        {
            nameText.text = name = creatureName;

            if (SystemUtility.IsDevice(DeviceType.Desktop))
            {
                if (EducationManager.Instance.IsEducational)
                {
                    shareButton.gameObject.SetActive(false);
                }
                else
                {
                    shareButton.gameObject.SetActive(!FactoryManager.Instance.LoadedCreatures.Contains(creatureName));
                }
            }
        }
        
        public void SetSharing(bool isSharing)
        {
            progress.SetActive(isSharing);
            shareButton.gameObject.SetActive(!isSharing);
        }
        #endregion
    }
}