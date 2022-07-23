// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class MiscUI : MonoBehaviour
    {
        public void SubscribeToYouTubeChannel()
        {
            Application.OpenURL("https://www.youtube.com/channel/UCGLR3v7NaV1t92dnzWZNSKA?sub_confirmation=1");
        }
        public void FollowTwitterAccount()
        {
            Application.OpenURL("https://twitter.com/daniellochner");
        }
        public void JoinDiscordServer()
        {
            Application.OpenURL("https://discord.gg/sJysbdu");
        }
        public void ViewGitHubSourceCode()
        {
            Application.OpenURL("https://github.com/daniellochner/Creature-Creator");

            DateTime releaseDate = new DateTime(2022, 8, 7);
            TimeSpan diff = releaseDate - DateTime.Now;
            if (diff > TimeSpan.Zero)
            {
                InformationDialog.Inform("Game Source Code", $"To prevent the puzzles from being spoilt, the source code to the game itself will release separately in:<br>{diff.Days} days, {diff.Hours} hours, {diff.Minutes} minutes and {diff.Seconds} seconds.");
            }
            else
            {
                Application.OpenURL("https://github.com/daniellochner/creature-creator-demo");
            }
        }
        public void Quit()
        {
            ConfirmationDialog.Confirm("Quit", "Are you sure you want to exit this application?", onYes: Application.Quit);
        }
    }
}