using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class DownloadedModsMenu : Dialog<DownloadedModsMenu>
    {
        public DownloadedModUI downloadedModPrefab;
        public SerializableDictionaryBase<FactoryItemType, ModTypeInfo> modTypeInfos;

        protected override void Start()
        {
            base.Start();
            foreach (var data in FactoryManager.Data.DownloadedItems)
            {
                AddMod(data.Value);
            }
            FactoryManager.Instance.OnDataDownloaded += AddMod;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (FactoryManager.Instance)
            {
                FactoryManager.Instance.OnDataDownloaded -= AddMod;
            }
        }

        private void Update()
        {
            foreach (var info in modTypeInfos.Values)
            {
                info.none.SetActive(info.root.childCount == 0);
            }
        }

        public void AddMod(FactoryData.DownloadedItemData data)
        {
            if (modTypeInfos.TryGetValue(data.Tag, out ModTypeInfo info))
            {
                var mod = Instantiate(downloadedModPrefab, info.root);
                mod.Setup(data);
            }
        }

        [Serializable]
        public class ModTypeInfo
        {
            public Transform root;
            public GameObject none;
        }
    }
}