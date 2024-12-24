// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

namespace DanielLochner.Assets.CreatureCreator
{
    public class FactoryItemUI : MonoBehaviour
    {
        #region Fields
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI upVotesText;
        public Image previewImg;
        public Button pageBtn;
        public Button subscribeBtn;
        public Button likeBtn;
        public Sprite addIcon;
        public Sprite removeIcon;
        public GameObject downloadBtn;
        public GameObject downloadingIcon;
        [Space]
        public GameObject info;
        public GameObject refreshIcon;
        public GameObject errorIcon;
        [Space]
        public GameObject subscribePanel;
        public GameObject downloadPanel;

        private Coroutine previewCoroutine;
        private bool isLiked, isDisliked, isSubscribed;
        private FactoryItem item;
        #endregion

        #region Properties
        private bool ShouldSubscribe => SystemUtility.IsDevice(DeviceType.Desktop) && !EducationManager.Instance.IsEducational;
        #endregion

        #region Methods
        private void Awake()
        {
            subscribePanel.SetActive(ShouldSubscribe);
            downloadPanel.SetActive(!ShouldSubscribe);
        }

        public void Setup(FactoryItem item)
        {
            this.item = item;

            nameText.text = item.name;
            upVotesText.text = item.upVotes.ToString();

            SetSubscribed(FactoryManager.Data.SubscribedItems.Contains(item.id));
            SetLiked(FactoryManager.Data.LikedItems.Contains(item.id));
            SetDisliked(FactoryManager.Data.DislikedItems.Contains(item.id));
            SetPreview(item.previewURL);
        }

        public void View()
        {
            FactoryItemMenu.Instance.View(item, this, previewImg.sprite, isSubscribed, isLiked, isDisliked);
        }
        public void Like()
        {
            if (!isLiked)
            {
                FactoryManager.Instance.LikeItem(item.id);
            }
            SetLiked(!isLiked);
        }
        public void Subscribe()
        {
            if (isSubscribed)
            {
                FactoryManager.Instance.UnsubscribeItem(item.id);
            }
            else
            {
                FactoryManager.Instance.SubscribeItem(item.id);
                Download(false);
            }
            SetSubscribed(!isSubscribed);
        }
        public void Download(bool notify)
        {
            if (!PremiumManager.Data.IsPremium && PremiumManager.Data.DownloadsToday >= 3)
            {
                InformationDialog.Inform(LocalizationUtility.Localize("mainmenu_premium_factory_title"), LocalizationUtility.Localize("mainmenu_premium_factory_message"), onOkay: delegate
                {
                    PremiumMenu.Instance.RequestNothing();
                });
            }
            else
            if (!FactoryManager.Instance.IsDownloadingItem)
            {
                SetDownloading(true);

                FactoryManager.Instance.DownloadItem(item.id, delegate (string name)
                    {
                        SetDownloading(false, false);

                        if (notify)
                        {
                            InformationDialog.Inform(LocalizationUtility.Localize("factory_download_title"), LocalizationUtility.Localize("factory_download_message", name));
                        }
                    },
                    delegate (string error)
                    {
                        SetDownloading(false);
                    });

                PremiumManager.Data.DownloadsToday++;
                PremiumManager.Instance.Save();
            }
            else
            {
                InformationDialog.Inform(LocalizationUtility.Localize("factory_downloading_title"), LocalizationUtility.Localize("factory_downloading_message"));
            }
        }

        public void SetLiked(bool isLiked)
        {
            this.isLiked = isLiked;
            uint likes = item.upVotes + (isLiked ? 1u : 0u);
            upVotesText.text = $"{likes}";
        }
        public void SetDisliked(bool isDisliked)
        {
            this.isDisliked = isDisliked;
        }
        public void SetSubscribed(bool isSubscribed)
        {
            this.isSubscribed = isSubscribed;
            subscribeBtn.image.sprite = isSubscribed ? removeIcon : addIcon;
        }

        public void SetPreview(string url)
        {
            this.StopStartCoroutine(SetPreviewRoutine(url), ref previewCoroutine);
        }
        private IEnumerator SetPreviewRoutine(string url)
        {
            refreshIcon.SetActive(true);

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D preview = ((DownloadHandlerTexture)request.downloadHandler).texture;
                previewImg.sprite = Sprite.Create(preview, new Rect(0, 0, preview.width, preview.height), new Vector2(0.5f, 0.5f));
                info.SetActive(true);
            }
            else
            {
                errorIcon.SetActive(true);
            }

            refreshIcon.SetActive(false);
        }

        private void SetDownloading(bool isDownloading, bool isDownloadable = true)
        {
            downloadingIcon.SetActive(isDownloading);
            downloadBtn.SetActive(!isDownloading && isDownloadable);

            if (ShouldSubscribe)
            {
                downloadPanel.SetActive(isDownloading);
                subscribePanel.SetActive(!isDownloading);
            }
        }
        #endregion
    }
}