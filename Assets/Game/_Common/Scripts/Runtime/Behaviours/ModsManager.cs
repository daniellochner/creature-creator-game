using System.Collections.Generic;
using System.IO;

namespace DanielLochner.Assets.CreatureCreator
{
    public class ModsManager : MonoBehaviourSingleton<ModsManager>
    {
        public List<string> CustomMapIds => GetCustomItemIds(CCConstants.MapsDir);
        public List<string> CustomBodyPartIds => GetCustomItemIds(CCConstants.BodyPartsDir);
        public List<string> CustomPatternIds => GetCustomItemIds(CCConstants.PatternsDir);

        public List<string> GetCustomItemIds(string path)
        {
            var itemIds = new List<string>();
            foreach (var dir in Directory.GetDirectories(path))
            {
                itemIds.Add(Path.GetFileNameWithoutExtension(dir));
            }
            return itemIds;
        }

        public bool HasRequiredMods(WorldMP world, out string reqMapId, out List<string> reqBodyPartIds, out List<string> reqPatternIds)
        {
            // Map
            reqMapId = "";
            if (!CustomMapIds.Contains(world.CustomMapId))
            {
                reqMapId = world.CustomMapId;
            }
            bool needMap = reqMapId != "";

            // Body Parts
            reqBodyPartIds = new List<string>();
            foreach (var bodyPartId in world.CustomBodyPartIds)
            {
                if (!string.IsNullOrEmpty(bodyPartId) && !CustomBodyPartIds.Contains(bodyPartId))
                {
                    reqBodyPartIds.Add(bodyPartId);
                }
            }
            bool needBodyPart = reqBodyPartIds.Count > 0;

            // Patterns
            reqPatternIds = new List<string>();
            foreach (var patternId in world.CustomPatternIds)
            {
                if (!string.IsNullOrEmpty(patternId) && !CustomPatternIds.Contains(patternId))
                {
                    reqPatternIds.Add(patternId);
                }
            }
            bool needPattern = reqPatternIds.Count > 0;

            return !(needMap || needBodyPart || needPattern);
        }
    }
}