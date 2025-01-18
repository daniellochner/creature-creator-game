using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DanielLochner.Assets.CreatureCreator.FactoryManager;

namespace DanielLochner.Assets.CreatureCreator
{
    public class ModsMenu : Dialog<ModsMenu>
    {
        public ModUI modPrefab;
        public Transform modsRoot;
        public GameObject modsNone;

        private List<ModUI> mods = new ();
        private Coroutine downloadCoroutine;

        public void Setup(Action onDownloaded)
        {
            this.StopStartCoroutine(DownloadRoutine(onDownloaded), ref downloadCoroutine);
            Open();
        }

        private IEnumerator DownloadRoutine(Action onDownloaded)
        {
            foreach (var modUI in mods)
            {
                if (!modUI.IsDownloaded)
                {
                    yield return modUI.DownloadRoutine();
                }
            }
            onDownloaded?.Invoke();
        }

        public bool AddMod(RequiredModData reqItem, FactoryItemType itemType)
        {
            return AddMod(new FactoryItem()
            {
                id = reqItem.id,
                timeUpdated = reqItem.version,
                tag = itemType
            });
        }
        public bool AddMod(FactoryItem item)
        {
            var modUI = mods.Find(x => x.Item.id == item.id);
            if (modUI == null)
            {
                modUI = Instantiate(modPrefab, modsRoot);
                modUI.Setup(item);
                mods.Add(modUI);
                modsNone.SetActive(false);
            }
            return true;
        }
    }
}