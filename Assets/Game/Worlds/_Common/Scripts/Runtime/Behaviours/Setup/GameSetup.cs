// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace DanielLochner.Assets.CreatureCreator
{
    [DefaultExecutionOrder(-1)]
    public class GameSetup : MonoBehaviourSingleton<GameSetup>, ISetupable
    {
        #region Fields
        [SerializeField] private GameObject[] toEnable;
        [SerializeField] private GameObject[] toDisable;
        [Space]
        [SerializeField] private NetworkObject playerPrefabL;
        [SerializeField] private NetworkObject playerPrefabR;
        [SerializeField] private Platform[] platforms;
        [SerializeField] private NetworkObject[] helpers;
        [SerializeField] private UnityEvent onTutorial;
        #endregion

        #region Properties
        public bool IsSetup { get; set; }

        public Platform StartingPlatform => platforms[WorldManager.Instance.World.SpawnPoint % platforms.Length];
        #endregion

        #region Methods
        private IEnumerator Start()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                yield return new WaitUntil(() => Player.Instance && Player.Instance.DataContainer.Data != null); // wait until the player has been replicated...
            }
            Setup();
        }
		public void SetPlatforms(Platform[] platforms)
		{
			this.platforms = platforms;
		}
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (NetworkManager.Singleton)
            {
                if (!NetworkConnectionManager.IsConnected)
                {
                    Shutdown();
                }

                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }

            IsSetup = false;
        }

        public void Setup()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                foreach (var cc in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    OnClientConnected(cc);
                }
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }

            if (WorldManager.Instance.IsMultiplayer)
            {
                SetupMP();
            }
            else
            {
                SetupSP();
            }

            Player.Instance.Editor.Platform = StartingPlatform;
            EditorManager.Instance.Setup();

            if (SettingsManager.Instance.ShowTutorial || WorldManager.Instance.IsTimed)
            {
                EditorManager.Instance.SetMode(EditorManager.EditorMode.Play, true);
                if (!WorldManager.Instance.IsTimed)
                {
                    onTutorial.Invoke();
                    SettingsManager.Data.Tutorial = false;
                }
            }
            else
            {
                EditorManager.Instance.SetMode(EditorManager.EditorMode.Build, true);
            }

            foreach (GameObject go in toEnable)
            {
                go.SetActive(true);
            }
            foreach (GameObject go in toDisable)
            {
                go.SetActive(false);
            }

            IsSetup = true;
        }
        public void SetupMP()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkHostManager.Instance.SpawnPosition = StartingPlatform.Position;
                NetworkHostManager.Instance.SpawnRotation = StartingPlatform.Rotation;

                foreach (NetworkObject helper in helpers)
                {
                    Instantiate(helper).Spawn(true);
                }
            }

            NetworkShutdownManager.Instance.OnUncontrolledShutdown += OnUncontrolledShutdown;
            NetworkShutdownManager.Instance.OnUncontrolledClientShutdown += OnUncontrolledClientShutdown;
            NetworkShutdownManager.Instance.OnUncontrolledHostShutdown += OnUncontrolledHostShutdown;

            NetworkInactivityManager.Instance.OnInactivityKick += OnInactivityKick;
            NetworkInactivityManager.Instance.OnInactivityWarn += OnInactivityWarn;



            WorldMP world = WorldManager.Instance.World as WorldMP;

            if (NetworkManager.Singleton.IsHost)
            {
                if (world.IsPrivate)
                {
                    InformationDialog.Inform(LocalizationUtility.Localize("cc_private-world_title"), LocalizationUtility.Localize("cc_private-world_message", world.Id, KeybindingsManager.Data.ViewPlayers), LocalizationUtility.Localize("cc_private-world_okay"), true, delegate
                    {
                        GUIUtility.systemCopyBuffer = world.Id;
                    });
                }
                if (world.SpawnNPC)
                {
                    foreach (NPCSpawner npc in NPCSpawner.WorldSpawners)
                    {
                        npc.Spawn();
                        npc.SpawnedNPC.GetComponent<AnimalAI>().PVE = world.EnablePVE;
                    }
                }
            }

            NetworkPlayersManager.Instance.Setup(world.Id);

            EditorManager.Instance.CreativeMode = world.Mode == Mode.Creative;
            EditorManager.Instance.CheckForProfanity = !world.AllowProfanity;

            PlayerChatManager.Instance.checkForProfanity = !world.AllowProfanity;
        }
        public void SetupSP()
        {
            WorldSP world = WorldManager.Instance.World as WorldSP;
            
            if (world.SpawnNPC)
            {
                foreach (NPCSpawner npc in NPCSpawner.WorldSpawners)
                {
                    npc.Spawn();
                    npc.SpawnedNPC.GetComponent<AnimalAI>().PVE = world.EnablePVE;
                }
            }

            Player.Instance.DeathMessenger.enabled = false;

            EditorManager.Instance.CreativeMode = world.Mode == Mode.Creative;
        }

        public void Shutdown()
        {
            if (WorldManager.Instance.IsMultiplayer)
            {
                ShutdownMP();
            }
            else
            {
                ShutdownSP();
            }

            WorldManager.Instance.World = null;
            MusicManager.Instance.FadeTo(null, 0f, 1f);
        }
        public void ShutdownMP()
        {
            NetworkShutdownManager.Instance.OnUncontrolledShutdown -= OnUncontrolledShutdown;
            NetworkShutdownManager.Instance.OnUncontrolledClientShutdown -= OnUncontrolledClientShutdown;
            NetworkShutdownManager.Instance.OnUncontrolledHostShutdown -= OnUncontrolledHostShutdown;
        }
        public void ShutdownSP()
        {

        }

        private void OnClientConnected(ulong clientId)
        {
            NetworkObject obj = Instantiate(clientId == NetworkManager.Singleton.LocalClientId ? playerPrefabL : playerPrefabR, StartingPlatform.Position, StartingPlatform.Rotation);
            obj.SpawnAsPlayerObject(clientId, true);
        }
        private void OnUncontrolledShutdown()
        {
            SceneManager.LoadScene("MainMenu");
        }
        private void OnUncontrolledClientShutdown()
        {
            InformationDialog.Inform(LocalizationUtility.Localize("disconnect_title"), LocalizationUtility.Localize("disconnect_message_you-lost-connection"));
        }
        private void OnUncontrolledHostShutdown()
        {
            InformationDialog.Inform(LocalizationUtility.Localize("disconnect_title"), LocalizationUtility.Localize("disconnect_message_host-lost-connection"));
        }
        private void OnInactivityKick()
        {
            InformationDialog.Instance.Close(true);
            InformationDialog.Inform(LocalizationUtility.Localize("disconnect_title"), LocalizationUtility.Localize("disconnect_message_inactivity"));
        }
        private void OnInactivityWarn(int warnTime)
        {
            InformationDialog.Inform(LocalizationUtility.Localize("inactivity_title"), LocalizationUtility.Localize("inactivity_message"), LocalizationUtility.Localize("inactivity_cancel"));
        }
        #endregion
    }
}