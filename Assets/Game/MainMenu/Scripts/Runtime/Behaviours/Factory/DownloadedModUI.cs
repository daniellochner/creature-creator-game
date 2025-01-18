using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DanielLochner.Assets.CreatureCreator
{
    public class DownloadedModUI : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public Button removeButton;

        public void Setup(FactoryData.DownloadedItemData data)
        {
            nameText.text = data.Name;

            removeButton.onClick.AddListener(delegate
            {
                FactoryManager.Instance.UnsubscribeItem(data.Id);
                FactoryManager.Instance.RemoveItem(data.Id, data.Tag);
                Destroy(gameObject);
            });
        }
    }
}