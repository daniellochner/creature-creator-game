// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class ShopMenu : MenuSingleton<ShopMenu>
    {
        #region Methods
        public void Visit()
        {
            Application.OpenURL("https://playcreature.com/merch");
        }
        #endregion
    }
}