using System;

namespace DanielLochner.Assets.CreatureCreator
{
    [Serializable]
    public class FactoryItemQuery
    {
        public FactoryItemType TagType;
        public FactorySortByType SortByType;
        public FactoryTimePeriodType TimePeriodType;
        public int NumPerPage;
        public int Page;
        public string SearchText = "";

        public override string ToString()
        {
            return $"{TagType}, {SortByType}, {TimePeriodType}, {NumPerPage}, {Page}, {SearchText}";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not FactoryItemQuery)
            {
                return false;
            }
            FactoryItemQuery other = obj as FactoryItemQuery;
            return TagType.Equals(other.TagType) && SortByType.Equals(other.SortByType) && TimePeriodType.Equals(other.TimePeriodType) && NumPerPage.Equals(other.NumPerPage) && Page.Equals(other.Page) && SearchText.Equals(other.SearchText);
        }
    }
}