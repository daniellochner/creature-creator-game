// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using UnityEngine;

#if UNITY_STANDALONE
using Steamworks;
#endif

namespace DanielLochner.Assets.CreatureCreator
{
    public class ProgressManager : DataManager<ProgressManager, Progress>
    {
        #region Properties
        public override string SALT
        {
            get
            {
#if UNITY_STANDALONE
                if (EducationManager.Instance.IsEducational)
                {
                    return SystemInfo.deviceUniqueIdentifier;
                }
                else
                {
                    return SteamUser.GetSteamID().ToString();
                }
#elif UNITY_IOS || UNITY_ANDROID
                return SystemInfo.deviceUniqueIdentifier;
#endif
            }
        }

        public string PrevCreatureData
        {
            get => PlayerPrefs.GetString("PREV_CREATURE_DATA");
            set => PlayerPrefs.SetString("PREV_CREATURE_DATA", value);
        }
        #endregion

        #region Methods
        protected override void Start()
        {
            base.Start();
            MigrateLegacy();
        }

        public override void Revert()
        {
            base.Revert();
            PrevCreatureData = null;
        }

        // Body Parts
        public void UnlockBodyPart(string bodyPartId)
        {
            if (!IsBodyPartUnlocked(bodyPartId))
            {
                Data.UnlockedBodyParts.Add(bodyPartId);
                Save();

                StatsManager.Instance.UnlockedBodyParts++;
            }
        }
        public bool IsBodyPartUnlocked(string bodyPartId)
        {
            return Data.UnlockedBodyParts.Contains(bodyPartId);
        }

        // Patterns
        public void UnlockPattern(string patternId)
        {
            if (!IsPatternUnlocked(patternId))
            {
                Data.UnlockedPatterns.Add(patternId);
                Save();

                StatsManager.Instance.UnlockedPatterns++;
            }
        }
        public bool IsPatternUnlocked(string patternId)
        {
            return Data.UnlockedPatterns.Contains(patternId);
        }

        // Completed Quests
        public void CompleteQuest(string questId)
        {
            if (!IsQuestCompleted(questId))
            {
                Data.CompletedQuests.Add(questId);
                Save();
            }
        }
        public bool IsQuestCompleted(string questId)
        {
            return Data.CompletedQuests.Contains(questId);
        }

        // Unlocked Maps
        public void UnlockMap(Map map)
        {
            if (!IsMapUnlocked(map))
            {
                Data.UnlockedMaps.Add(map);
                Save();
            }
        }
        public bool IsMapUnlocked(Map map)
        {
            return Data.UnlockedMaps.Contains(map) || (map == Map.Island || map == Map.Custom);
        }

        // Reached Peaks
        public void ReachPeak(Map map)
        {
            if (!IsPeakReached(map))
            {
                Data.ReachedPeaks.Add(map);
                Save();
            }
        }
        public bool IsPeakReached(Map map)
        {
            return Data.ReachedPeaks.Contains(map);
        }

        #region Legacy
        private static readonly Map[] MAPS = new Map[]
        {
            Map.Island,
            Map.Farm,
            Map.Sandbox,
            Map.Cave,
            Map.City
        };
        private static readonly string[] QUESTS = new string[]
        {
            // Island
            "27dh3g2",

            // Farm
            "9n5pdf6",
            "j5pz7s0",
            "8s7s83i",
            "lo4zz8f",
            "01lfpx7",
            "mn72a0b",
            "f8s5x02",

            // Sandbox
            "8nsgy3m",
            "9js6hk4",

            // Cave
            "k2nx0l",

            // City
            "fkfnwa",
            "s9f2ln"
        };

        private void MigrateLegacy()
        {
            // Completed Quests
            foreach (string questId in QUESTS)
            {
                if (PlayerPrefs.GetInt($"quest_{questId}") == 1)
                {
                    CompleteQuest(questId);
                }
            }

            // Unlocked Maps
            foreach (Map map in MAPS)
            {
                if (PlayerPrefs.GetInt($"map_unlocked_{map}".ToLower()) == 1)
                {
                    UnlockMap(map);
                }
            }
            UnlockMap(Map.Island);

            // Reached Peaks
            foreach (Map map in MAPS)
            {
                if (PlayerPrefs.GetInt($"hp_{map}".ToUpper()) == 1)
                {
                    ReachPeak(map);
                }
            }
        }
        #endregion
        #endregion
    }
}