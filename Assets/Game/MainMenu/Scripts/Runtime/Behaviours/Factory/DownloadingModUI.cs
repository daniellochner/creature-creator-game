using System.Collections;
using TMPro;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class DownloadingModUI : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public GameObject downloadingIcon;
        public GameObject errorIcon;
        public GameObject downloadedIcon;

        public FactoryItem Item { get; private set; }

        public bool IsDownloaded => downloadedIcon.activeSelf;

        public void Setup(FactoryItem item)
        {
            Item = item;

            name = item.id.ToString();
            nameText.text = $"({item.tag}) {item.id}";
        }

        public IEnumerator DownloadRoutine()
        {
            DownloadStatus status = DownloadStatus.Downloading;
            downloadingIcon.SetActive(true);

            FactoryManager.Instance.DownloadItem(Item, delegate
            {
                status = DownloadStatus.Downloaded;
            },
            delegate (string reason)
            {
                status = DownloadStatus.Error;
            });

            yield return new WaitUntil(() => status != DownloadStatus.Downloading);
            downloadingIcon.SetActive(false);

            if (status == DownloadStatus.Downloaded)
            {
                downloadedIcon.SetActive(true);
            }
            else
            {
                errorIcon.SetActive(true);
            }
        }

        public enum DownloadStatus
        {
            Downloading,
            Error,
            Downloaded
        }
    }
}