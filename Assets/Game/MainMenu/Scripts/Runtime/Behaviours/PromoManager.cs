// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System.Collections;
using TMPro;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DanielLochner.Assets.CreatureCreator
{
    public class PromoManager : MonoBehaviourSingleton<PromoManager>
    {
        [SerializeField] private Image promoIconImg;
        [SerializeField] private TextMeshProUGUI promoTitleText;
        [SerializeField] private Button promoButton;
        [SerializeField] private CanvasGroup promoCanvasGroup;

        public IEnumerator Setup(bool fetchConfig)
        {
            if (fetchConfig)
            {
                yield return RemoteConfigUtility.FetchConfigRoutine();
            }

            PromoData promoData = JsonUtility.FromJson<PromoData>(RemoteConfigService.Instance.appConfig.GetJson("promo_data"));
            if (promoData.title != "")
            {
                promoTitleText.text = promoData.title;

                UnityWebRequest request = UnityWebRequestTexture.GetTexture(promoData.iconURL);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D preview = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    promoIconImg.sprite = Sprite.Create(preview, new Rect(0, 0, preview.width, preview.height), new Vector2(0.5f, 0.5f));
                    promoButton.onClick.AddListener(delegate 
                    {
                        Application.OpenURL(promoData.url);
                    });
                    promoButton.gameObject.SetActive(true);
                    StartCoroutine(promoCanvasGroup.FadeRoutine(true, 0.25f));
                }
            }
        }

        public class PromoData
        {
            public string title;
            public string iconURL;
            public string url;
        }
    }
}