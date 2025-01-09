using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class FactoryContentUI : MonoBehaviour, ISetupable
    {
        #region Fields
        public FactoryItemType tagType;
        public string titleId;

        public FactoryItemUI factoryItemUIPrefab;

        public GameObject pageLoadingGO;
        public GameObject loadingGO;
        public CanvasGroup contentCG;
        public GameObject viewGO;
        public GameObject noSearchGO;
        public GameObject searchGO;

        public TextMeshProUGUI searchText;
        public TextMeshProUGUI pageText;
        public TextMeshProUGUI numResultsText;
        public TMP_Dropdown sortByDropdown;
        public TMP_Dropdown timePeriodDropdown;
        public CanvasGroup prevBtnCG;
        public CanvasGroup nextBtnCG;
        public CellSizeCalculator csCalculator;

        private List<FactoryItemUI> addedItems = new();
        private FactoryItemQuery currentItemQuery = new();
        private int totalResults = -1;
        #endregion

        #region Properties
        public RectTransform RectTransform
        {
            get => transform as RectTransform;
        }

        public string Title
        {
            get => LocalizationUtility.Localize(titleId);
        }

        public bool IsLoadingPage
        {
            set
            {
                pageLoadingGO.SetActive(value);
                pageText.gameObject.SetActive(!value);

                contentCG.interactable = !value;
            }
        }
        public bool IsLoading
        {
            set
            {
                loadingGO.SetActive(value);
                viewGO.SetActive(!value);
            }
        }

        private int TotalPages
        {
            get => totalResults / currentItemQuery.NumPerPage;
        }

        public string SearchText
        {
            get => searchText.text;
            set
            {
                searchText.text = currentItemQuery.SearchText = value;

                bool hasText = !string.IsNullOrEmpty(value);
                searchGO.SetActive(hasText);
                noSearchGO.SetActive(!hasText);
            }
        }

        public bool IsSetup { get; set; }
        #endregion

        #region Methods
        public void Setup()
        {
            if (IsSetup)
            {
                return;
            }

            IsSetup = true;
            IsLoading = true;

            currentItemQuery = new()
            {
                Page = 0,
                NumPerPage = SystemUtility.IsDevice(DeviceType.Handheld) ? 12 : 50,
                SearchText = "",
                SortByType = FactorySortByType.MostPopular,
                TimePeriodType = FactoryTimePeriodType.ThisMonth,
                TagType = tagType
            };

            GetItems();
        }

        public void PrevPage()
        {
            if (currentItemQuery.Page > 0)
            {
                OnPageChanged(currentItemQuery.Page - 1);
            }
        }
        public void NextPage()
        {
            if (currentItemQuery.Page < TotalPages)
            {
                OnPageChanged(currentItemQuery.Page + 1);
            }
        }
        public void Search()
        {
            InputDialog.Input(LocalizationUtility.Localize("factory_search_title"), onSubmit: delegate (string text)
            {
                SearchText = text;
                currentItemQuery.Page = 0;
                GetItems();
            });
        }
        public void ClearSearch()
        {
            SearchText = "";
            currentItemQuery.Page = 0;
            GetItems();
        }

        public void OnPageChanged(int page)
        {
            currentItemQuery.Page = page;
            SetPageUI(currentItemQuery.Page, currentItemQuery.NumPerPage);

            GetItems();
        }
        public void OnSortByChanged(int index)
        {
            currentItemQuery.SortByType = (FactorySortByType)index;
            GetItems();
        }
        public void OnTimePeriodChanged(int index)
        {
            currentItemQuery.TimePeriodType = (FactoryTimePeriodType)index;
            GetItems();
        }

        private void AddItemUI(FactoryItem item)
        {
            var factoryItemUI = Instantiate(factoryItemUIPrefab, contentCG.transform);
            factoryItemUI.Setup(item);
            addedItems.Add(factoryItemUI);
        }
        private void SetPageUI(int page, int numPerPage)
        {
            int rangeMin = Mathf.Min(page * numPerPage + 1, totalResults);
            int rangeMax = Mathf.Min(rangeMin + numPerPage - 1, totalResults);
            numResultsText.text = $"{rangeMin}-{rangeMax} / {totalResults}";

            pageText.text = $"{page + 1}";

            bool canPrev = currentItemQuery.Page > 0;
            prevBtnCG.alpha = canPrev ? 1f : 0.25f;
            prevBtnCG.interactable = prevBtnCG.blocksRaycasts = canPrev;

            bool canNext = currentItemQuery.Page <= TotalPages - 1;
            nextBtnCG.alpha = canNext ? 1f : 0.25f;
            nextBtnCG.interactable = nextBtnCG.blocksRaycasts = canNext;
        }

        private void GetItems()
        {
            IsLoadingPage = true;

            FactoryManager.Instance.GetItems(currentItemQuery, OnLoaded, OnFailed);
        }

        private void OnLoaded(List<FactoryItem> items, uint total)
        {
            totalResults = (int)total;
            SetPageUI(currentItemQuery.Page, currentItemQuery.NumPerPage);

            IsLoading = IsLoadingPage = false;

            foreach (var itemUI in addedItems)
            {
                Destroy(itemUI.gameObject);
            }
            addedItems.Clear();

            foreach (var item in items)
            {
                AddItemUI(item);
            }
        }
        private void OnFailed(string error)
        {
            Debug.Log(error);

            IsLoading = IsLoadingPage = false;
        }
        #endregion
    }
}