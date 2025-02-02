using UnityEngine;
using UnityEngine.Events;

namespace DanielLochner.Assets
{
    [DefaultExecutionOrder(-1)]
    public class MinimapManager : MonoBehaviourSingleton<MinimapManager>
    {
        #region Fields
        public Rect mapBounds;
        [Space]
        [SerializeField] private Menu minimap;
        [SerializeField] private CanvasGroup minimapCG;
        [SerializeField] private SimpleZoom.SimpleZoom minimapZoom;
        [SerializeField] private RectTransform map;
        [SerializeField] private RectTransform icons;
        [SerializeField] private MinimapIconUI minimapIconPrefab;

        private MinimapIconUI target;
        #endregion

        #region Properties
        public Menu Minimap => minimap;

        private bool IsEnabled => minimap.gameObject.activeSelf;
        #endregion

        #region Methods
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(new Vector3(mapBounds.x, 0f, mapBounds.y), new Vector3(mapBounds.size.x, 0f, mapBounds.size.y));
        }

        public MinimapIconUI Add(Sprite icon, Color color, UnityAction onClick, bool isButton, bool isTarget, bool isImportant)
        {
            MinimapIconUI minimapIconUI = Instantiate(minimapIconPrefab, icons);
            minimapIconUI.Setup(icon, color, onClick, isButton);
            if (isTarget)
            {
                target = minimapIconUI;
            }
            if (isImportant)
            {
                minimapIconUI.transform.SetAsLastSibling();
            }
            else
            {
                minimapIconUI.transform.SetAsFirstSibling();
            }
            return minimapIconUI;
        }
        public void Track(MinimapIcon icon, MinimapIconUI iconUI, bool lockPos = false, bool lockRot = false)
        {
            if (!IsEnabled) return;

            Vector2 wPos = mapBounds.center + new Vector2(icon.transform.position.x, icon.transform.position.z) - new Vector2(mapBounds.x, mapBounds.y);
            Vector2 nPos = Rect.PointToNormalized(mapBounds, wPos);

            if (!minimap.IsOpen && target == iconUI)
            {
                minimapZoom.SetPivot(nPos);
                minimapZoom.Content.anchoredPosition = Vector2.zero;
            }

            if (!lockPos)
            {
                Vector2 aPos = Rect.NormalizedToPoint(map.rect, nPos) - map.rect.size / 2f;
                iconUI.RectTransform.anchoredPosition = aPos;
            }
            if (!lockRot)
            {
                iconUI.RectTransform.rotation = Quaternion.Euler(0, 0, -icon.transform.rotation.eulerAngles.y);
            }
        }

        public void SetVisibility(bool isVisible)
        {
            minimapCG.alpha = isVisible ? 1f : 0f;
            minimapCG.blocksRaycasts = minimapCG.interactable = isVisible;
        }
        #endregion
    }
}