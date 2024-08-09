// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Friends.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DanielLochner.Assets.CreatureCreator
{
    public class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        #region Properties
        public World World
        {
            get;
            set;
        }

        public bool EnablePVP
        {
            get
            {
                if (!IsMultiplayer)
                {
                    return true;
                }

                if (MinigameManager.Instance.CurrentPad != null)
                {
                    return false;
                }

                if (MinigameManager.Instance.CurrentMinigame != null)
                {
                    return MinigameManager.Instance.CurrentMinigame.EnablePVP;
                }

                return (World as WorldMP).EnablePVP;
            }
        }
        public bool IsMultiplayer => World is WorldMP;
        public bool IsTimed => World.Mode == Mode.Timed;
        public bool IsCreative => World.Mode == Mode.Creative;
        public bool IsUsingTeleport { get; set; }
        #endregion

        #region Methods
        private void Start()
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientStarted += OnClientStarted;

            NetworkShutdownManager.Instance.OnShutdown += OnShutdown;
        }

        private void OnServerStarted()
        {
            NetworkManager.Singleton.SceneManager.OnLoad += OnLoad;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;

            NetworkManager.Singleton.SceneManager.LoadScene(World.MapName, LoadSceneMode.Single);
        }
        private void OnClientStarted()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.SceneManager.OnLoad += OnLoad;
            }

            if (IsMultiplayer)
            {
                FriendsManager.Instance.SetStatus(Availability.Online, (World as WorldMP).Id);
            }
        }

        private void OnLoad(ulong clientId, string nextScene, LoadSceneMode loadSceneMode, AsyncOperation operation)
        {
            string prevScene = SceneManager.GetActiveScene().name;

            if (IsUsingTeleport)
            {
                TeleportManager.Instance.OnLeave(prevScene, nextScene);
            }

            LoadingManager.Instance.StartCoroutine(LoadingManager.Instance.LoadRoutine(operation, delegate
            {
                if (IsUsingTeleport)
                {
                    TeleportManager.Instance.OnEnter(prevScene, nextScene);
                    IsUsingTeleport = false;
                }
            }));
        }
        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (GameSetup.Instance && !GameSetup.Instance.IsSetup)
            {
                GameSetup.Instance.Setup();
            }
        }
        private void OnShutdown()
        {
            FriendsManager.Instance.SetStatus(Availability.Online, null);

            IsUsingTeleport = false;
        }
        
        public bool IsBodyPartUnlocked(string id)
        {
            if (World == null)
            {
                return ProgressManager.Instance.IsBodyPartUnlocked(id);
            }

            switch (World.Mode)
            {
                case Mode.Adventure:
                    return ProgressManager.Instance.IsBodyPartUnlocked(id);

                case Mode.Timed:
                    return TimedManager.Instance.IsBodyPartUnlocked(id);

                case Mode.Creative:
                default:
                    return true;
            }
        }
        public bool IsPatternUnlocked(string id)
        {
            if (World == null)
            {
                return ProgressManager.Instance.IsPatternUnlocked(id);
            }

            switch (World.Mode)
            {
                case Mode.Adventure:
                    return ProgressManager.Instance.IsPatternUnlocked(id);

                case Mode.Timed:
                    return TimedManager.Instance.IsPatternUnlocked(id);

                case Mode.Creative:
                default:
                    return true;
            }
        }
        public bool IsQuestCompleted(string id)
        {
            if (World == null)
            {
                return ProgressManager.Instance.IsQuestCompleted(id);
            }

            switch (World.Mode)
            {
                case Mode.Adventure:
                    return ProgressManager.Instance.IsQuestCompleted(id);

                case Mode.Timed:
                    return TimedManager.Instance.IsQuestCompleted(id);

                case Mode.Creative:
                default:
                    return true;
            }
        }
        #endregion
    }
}