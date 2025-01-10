using System.IO;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public static class CCConstants
    {
        public static string CreaturesDir => Path.Combine(Application.persistentDataPath, "creature");

        public static string ModsDir => Path.Combine(Application.persistentDataPath, "mods");
        public static string MapsDir => GetItemsDir(FactoryItemType.Map);
        public static string BodyPartsDir => GetItemsDir(FactoryItemType.BodyPart);
        public static string PatternsDir => GetItemsDir(FactoryItemType.Pattern);
        public static string GetItemsDir(FactoryItemType itemType) => Path.Combine(ModsDir, itemType.ToString().ToLower());

        public static uint AppId => 1990050;
    }
}