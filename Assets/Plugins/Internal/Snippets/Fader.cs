// Fader
// Copyright (c) Daniel Lochner

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DanielLochner.Assets
{
    public class Fader : MonoBehaviourSingleton<Fader>
    {
        #region Fields
        private static int maximumSortingOrder = 9999;

        private CanvasGroup fadeCanvasGroup;
        #endregion

        #region Methods
        public static void FadeInOut(float fadeDuration, UnityAction onFade = null)
        {
            Fade(true, fadeDuration, delegate
            {
                onFade?.Invoke();
                Fade(false, fadeDuration);
            });
        }

        public static void Fade(bool targetVisibility, float fadeDuration, UnityAction onFade = null)
        {
            if (Instance == null)
            {
                // Instantiate GameObject
                GameObject fadeCanvasGO = new GameObject("Fade");
                fadeCanvasGO.transform.SetAsLastSibling();
                fadeCanvasGO.AddComponent<DoNotDestroy>();
                fadeCanvasGO.AddComponent<Fader>(); // Singleton set on Awake()

                // Add Canvas
                Canvas fadeCanvas = fadeCanvasGO.AddComponent<Canvas>();
                fadeCanvas.sortingOrder = maximumSortingOrder;
                fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // Add RawImage
                RawImage rawImage = fadeCanvasGO.AddComponent<RawImage>();
                rawImage.color = Color.black;

                // Add CanvasGroup
                Instance.fadeCanvasGroup = fadeCanvasGO.AddComponent<CanvasGroup>();
                Instance.fadeCanvasGroup.alpha = targetVisibility ? 0 : 1;
            }

            // Fade CanvasGroup In/Out
            Instance.StartCoroutine(FadeRoutine(targetVisibility, fadeDuration, onFade));
        }

        private static IEnumerator FadeRoutine(bool targetVisibility, float fadeDuration, UnityAction onFade = null)
        {
            yield return new WaitForEndOfFrame(); // Start fading on next frame.

            yield return Instance.StartCoroutine(Instance.fadeCanvasGroup.FadeRoutine(targetVisibility, fadeDuration, true, onFade));
        }
        #endregion
    }
}