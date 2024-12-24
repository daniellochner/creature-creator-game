using System.Collections;
using TMPro;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class ModUI : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public GameObject downloadingIcon;
        public GameObject errorIcon;
        public GameObject downloadedIcon;
        private string modId;

        public bool IsDownloaded => downloadedIcon.activeSelf;

        public void Setup(string modId, FactoryItemType type)
        {
            this.modId = name = modId;
            nameText.text = $"({type}) {modId}";
        }

        public IEnumerator DownloadRoutine()
        {
            DownloadStatus status = DownloadStatus.Downloading;
            downloadingIcon.SetActive(true);

            FactoryManager.Instance.DownloadItem(ulong.Parse(modId), delegate (string text)
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