using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DanielLochner.Assets.CreatureCreator
{
    public class FactoryItemMenu : MenuSingleton<FactoryItemMenu>
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI authorText;
        public TextMeshProUGUI timeCreatedText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI upVotesText;
        public TextMeshProUGUI downVotesText;
        public GameObject downloadBtn;
        public GameObject downloadingIcon;
        public Button authorBtn;

        public Image iconImg;
        public Image subscribeImg;
        public Sprite addIcon;
        public Sprite removeIcon;

        public GameObject previewIcon;
        public GameObject refreshIcon;
        public GameObject errorIcon;

        public GameObject subscribePanel;
        public GameObject downloadPanel;

        private FactoryItemUI itemUI;


        private void Update()
        {
            if (IsOpen)
            {
                downloadBtn.SetActive(itemUI.downloadBtn.activeSelf);
                downloadingIcon.SetActive(itemUI.downloadingIcon.activeSelf);

                subscribeImg.sprite = itemUI.IsSubscribed ? removeIcon : addIcon;

                subscribePanel.SetActive(itemUI.subscribePanel.activeSelf);
                downloadPanel.SetActive(itemUI.downloadPanel.activeSelf);

                uint likes = itemUI.Item.upVotes + (itemUI.IsLiked ? 1u : 0u);
                upVotesText.text = $"{likes}";

                uint dislikes = itemUI.Item.downVotes + (itemUI.IsDisliked ? 1u : 0u);
                downVotesText.text = $"{dislikes}";
            }
        }

        public void View(FactoryItemUI itemUI)
        {
            if (this.itemUI != itemUI)
            {
                this.itemUI = itemUI;

                nameText.text = itemUI.Item.name;
                descriptionText.text = itemUI.Item.description;
                upVotesText.text = itemUI.Item.upVotes.ToString();
                downVotesText.text = itemUI.Item.downVotes.ToString();
                timeCreatedText.text = DateTimeUtility.UnixTimeStampToDateTime(itemUI.Item.timeCreated).ToString();
                iconImg.sprite = itemUI.previewImg.sprite;

                if (FactoryManager.Data.CachedUsernames.TryGetValue(itemUI.Item.creatorId, out string username))
                {
                    SetCreator(username, false);
                }
                else
                {
                    SetCreator($"[{itemUI.Item.creatorId}]", true);
                }
            }

            Open();
            Update();
        }

        public void Like()
        {
            itemUI.Like();
        }
        public void Dislike()
        {
            itemUI.Dislike();
        }
        public void Subscribe()
        {
            itemUI.Subscribe();
        }
        public void Download()
        {
            itemUI.Download(true);
        }
        public void ViewMore()
        {
            FactoryManager.Instance.ViewWorkshopItem(itemUI.Item.id);
        }

        private void SetCreator(string username, bool interactable)
        {
            authorText.text = $"By {username}";
            authorBtn.interactable = interactable;
        }

        public void LoadCreatorUsername()
        {
            FactoryManager.Instance.DownloadUsername(itemUI.Item.creatorId, delegate (string username)
            {
                SetCreator(username, false);
            },
            delegate (string error)
            {
                Debug.Log(error);
            });
        }
    }
}