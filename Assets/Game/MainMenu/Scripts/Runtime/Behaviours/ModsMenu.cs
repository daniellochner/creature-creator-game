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

        public bool AddMod(string modId, FactoryItemType type)
        {
            var modUI = mods.Find(x => x.name == modId);
            if (modUI == null)
            {
                modUI = Instantiate(modPrefab, modsRoot);
                modUI.Setup(modId, type);
                mods.Add(modUI);
                modsNone.SetActive(false);
            }
            return true;
        }
    }
}