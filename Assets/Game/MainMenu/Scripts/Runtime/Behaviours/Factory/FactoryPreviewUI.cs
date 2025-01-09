// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DanielLochner.Assets.CreatureCreator
{
    public class FactoryPreviewUI : MonoBehaviour, IPointerEnterHandler
    {
        #region Fields
        [SerializeField] private GameObject factoryItemUIPrefab;
        [SerializeField] private GameObject moreUIPrefab;
        [SerializeField] private Toggle togglePrefab;
        [SerializeField] private SimpleScrollSnap.SimpleScrollSnap factoryScrollSnap;
        [SerializeField] private float carouselTime;
        [SerializeField] private int maxItemsInCarousel;
        [SerializeField] private RectTransform pagination;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private CanvasGroup canvasGroup;

        private bool hasEntered;
        #endregion

        #region Methods
        private void Start()
        {
            if ((Application.internetReachability != NetworkReachability.NotReachable) && !SettingsManager.Instance.ShowTutorial)
            {
                FactoryItemQuery query = new()
                {
                    TagType = FactoryItemType.Creature,
                    SortByType = FactorySortByType.MostPopular,
                    TimePeriodType = FactoryTimePeriodType.ThisWeek,
                    NumPerPage = maxItemsInCarousel,
                    Page = 0
                };
                FactoryManager.Instance.GetItems(query, OnLoaded, OnFailed);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hasEntered = true;
        }

        public void OnLoaded(List<FactoryItem> items, uint total)
        {
            for (int i = 0; i < items.Count && i < maxItemsInCarousel; i++)
            {
                Instantiate(togglePrefab, pagination).group = toggleGroup;

                FactoryItemUI factoryItemUI = factoryScrollSnap.AddToBack(factoryItemUIPrefab).GetComponent<FactoryItemUI>();
                factoryItemUI.Setup(items[i]);
            }

            Instantiate(togglePrefab, pagination).group = toggleGroup;

            Button moreUI = factoryScrollSnap.AddToBack(moreUIPrefab).GetComponent<Button>();
            moreUI.onClick.AddListener(delegate
            {
                FactoryManager.Instance.ViewWorkshop();
            });

            if (total > 0)
            {
                StartCoroutine(canvasGroup.FadeRoutine(true, 0.25f));
                StartCoroutine(CarouselRoutine());
            }
        }

        public void OnFailed(string error)
        {
            Debug.Log(error);
        }

        private IEnumerator CarouselRoutine()
        {
            while (!hasEntered)
            {
                yield return new WaitForSeconds(carouselTime);
                if (!hasEntered)
                {
                    factoryScrollSnap.GoToNextPanel();
                }
            }
        }
        #endregion
    }
}