using System.Collections.Generic;
using System.IO;

namespace DanielLochner.Assets.CreatureCreator
{
    public class ModsManager : MonoBehaviourSingleton<ModsManager>
    {
        public List<string> GetCustomItemIds(FactoryItemType type)
        {
            var itemIds = new List<string>();
            foreach (var dir in Directory.GetDirectories(CCConstants.GetItemsDir(type)))
            {
                itemIds.Add(Path.GetFileNameWithoutExtension(dir));
            }
            return itemIds;
        }

        public bool HasRequiredMods(WorldMP world, out RequiredModData reqMap, out List<RequiredModData> reqBodyParts, out List<RequiredModData> reqPatterns)
        {
            // Map
            reqMap = null;
            if (IsRequiredModDownloaded(world.CustomMap))
            {
                reqMap = world.CustomMap;
            }
            bool needMap = reqMap != null;

            // Body Parts
            reqBodyParts = new List<RequiredModData>();
            foreach (var bodyPart in world.CustomBodyParts)
            {
                if (IsRequiredModDownloaded(bodyPart))
                {
                    reqBodyParts.Add(bodyPart);
                }
            }
            bool needBodyPart = reqBodyParts.Count > 0;

            // Patterns
            reqPatterns = new List<RequiredModData>();
            foreach (var pattern in world.CustomPatterns)
            {
                if (IsRequiredModDownloaded(pattern))
                {
                    reqPatterns.Add(pattern);
                }
            }
            bool needPattern = reqPatterns.Count > 0;

            return !(needMap || needBodyPart || needPattern);
        }

        private bool IsRequiredModDownloaded(RequiredModData reqMod)
        {
            return (reqMod != null) && (!FactoryManager.Data.DownloadedItems.TryGetValue(reqMod.id, out FactoryData.DownloadedItemData item) || item.Version != reqMod.version);
        }
    }
}