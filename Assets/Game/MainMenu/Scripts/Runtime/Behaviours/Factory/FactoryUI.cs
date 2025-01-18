using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class FactoryUI : MonoBehaviour
    {
        public FactoryContentUI[] contents;
        public SimpleScrollSnap.SimpleScrollSnap factorySSS;
        public SimpleScrollSnap.SimpleScrollSnap modsMenuSSS;

        public void ViewPC()
        {
            Application.OpenURL("https://store.steampowered.com/app/1990050/Creature_Creator/");
        }
        public void ViewContent()
        {
            FactoryContentMenu.Instance.View(contents[factorySSS.SelectedPanel]);
        }
        public void ViewDownloaded()
        {
            DownloadedModsMenu.Instance.Open();
            modsMenuSSS.GoToPanel(factorySSS.SelectedPanel);
        }
    }
}