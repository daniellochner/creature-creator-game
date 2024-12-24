// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using ProfanityDetector;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System;
using System.Text;
using System.Security.Cryptography;
using Unity.Netcode.Transports.UTP;
using Unity.Services.RemoteConfig;
using LobbyPlayer = Unity.Services.Lobbies.Models.Player;
using System.IO;

namespace DanielLochner.Assets.CreatureCreator
{
    public class MultiplayerUI : MonoBehaviour
    {
        #region Fields
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private BlinkingText statusBT;
        [SerializeField] private Button createButton;
        [SerializeField] private Menu multiplayerMenu;
        [SerializeField] private Menu multiplayerHintMenu;
        [SerializeField] private SimpleScrollSnap.SimpleScrollSnap multiplayerSSS;

        [Header("Join")]
        [SerializeField] private WorldUI worldUIPrefab;
        [SerializeField] private RectTransform worldsRT;
        [SerializeField] private GameObject noneGO;
        [SerializeField] private GameObject refreshGO;
        [SerializeField] private Button refreshButton;
        [SerializeField] private TMP_InputField lobbyCodeInputField;

        [Header("Create")]
        [SerializeField] private TMP_InputField worldNameInputField;
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private OptionSelector mapOS;
        [SerializeField] private OptionSelector modeOS;
        [SerializeField] private OptionSelector customMapOS;
        [SerializeField] private OptionSelector spawnPointOS;
        [SerializeField] private CanvasGroup customMapCG;
        [SerializeField] private GameObject customMapGO;
        [SerializeField] private CanvasGroup spawnPointCG;
        [SerializeField] private OptionSelector visibilityOS;
        [SerializeField] private CanvasGroup passwordCG;
        [SerializeField] private Toggle passwordToggle;
        [SerializeField] private Slider maxPlayersSlider;
        [SerializeField] private Toggle pvpToggle;
        [SerializeField] private Toggle pveToggle;
        [SerializeField] private CanvasGroup pveCG;
        [SerializeField] private Toggle npcToggle;
        [SerializeField] private Toggle profanityToggle;
        [SerializeField] private Image sortByIcon;
        [SerializeField] private MapUI mapUI;
        [SerializeField] private bool showComingSoon;

        private ProfanityFilter filter = new ProfanityFilter();
        private SHA256 sha256 = SHA256.Create();
        private bool isConnecting, isRefreshing, isSortedByAscending = true;
        private Coroutine updateStatusCoroutine;
        private int refreshCount;
        #endregion

        #region Properties
        public bool IsConnectedToInternet
        {
            get
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    UpdateStatus(LocalizationUtility.Localize("network_status_internet"), Color.white);
                    return false;
                }
                return true;
            }
        }
        public bool IsValidPlayer
        {
            get
            {
                string username = SettingsManager.Data.OnlineUsername;
                if (string.IsNullOrEmpty(username))
                {
                    UpdateStatus(LocalizationUtility.Localize("network_status_username"), Color.white);
                    return false;
                }
                if (filter.ContainsProfanity(username))
                {
                    UpdateStatus(LocalizationUtility.Localize("network_status_profanity"), Color.white);
                    return false;
                }
                if (username.Length > 16)
                {
                    UpdateStatus(LocalizationUtility.Localize("network_status_username-length"), Color.white);
                    return false;
                }
                return true;
            }
        }
        public bool IsValidWorldName
        {
            get
            {
                string worldName = worldNameInputField.text;
                if (string.IsNullOrEmpty(worldName))
                {
                    UpdateStatus(LocalizationUtility.Localize("network_status_world-name"), Color.white);
                    return false;
                }
                if (worldName.Length > 32)
                {
                    UpdateStatus(LocalizationUtility.Localize("network_status_world-name-length"), Color.white);
                    return false;
                }
                if (filter.ContainsProfanity(worldName))
                {
                    UpdateStatus(LocalizationUtility.Localize("network_status_world-name-profanity"), Color.white);
                    return false;
                }
                return true;
            }
        }

        private bool IsConnecting
        {
            get => isConnecting;
            set
            {
                isConnecting = value;
                createButton.interactable = !isConnecting;

                foreach (Transform worldRT in worldsRT)
                {
                    worldRT.GetComponent<WorldUI>().JoinButton.interactable = !isConnecting;
                }
            }
        }
        private bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                isRefreshing = value;
                refreshGO.SetActive(isRefreshing);
            }
        }
        #endregion

        #region Methods
        private void Start()
        {
            Setup();
        }

        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
        }
        private void OnDisable()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            }
            statusText.text = "";
        }

        private void Setup()
        {
            var ignoredMaps = new List<Map>()
            {
                Map.ComingSoon
            };
            var hasCustom = ModsManager.Instance.CustomMapIds.Count > 0;
            if (!hasCustom)
            {
                ignoredMaps.Add(Map.Custom);
            }
            mapOS.SetupUsingEnum<Map>(ignoredMaps.ToArray());
            mapOS.Select(Map.Island, false);
            mapOS.OnSelected.AddListener(delegate (int option)
            {
                bool isCustom = (Map)option == Map.Custom;
                customMapCG.SetEnabled(isCustom);

                if (!isCustom)
                {
                    int spawnPoints = DatabaseManager.GetDatabaseEntry<MapData>("Maps", ((Map)option).ToString())?.SpawnPoints ?? 1;

                    spawnPointOS.Options.Clear();
                    for (int i = 0; i < spawnPoints; i++)
                    {
                        spawnPointOS.Options.Add(new OptionSelector.Option()
                        {
                            Id = $"P{i + 1}"
                        });
                    }
                    spawnPointOS.Select(0);
                    spawnPointCG.SetEnabled(spawnPoints > 1);
                }
                else
                {
                    spawnPointCG.SetEnabled(false);
                }
            });
            customMapGO.SetActive(hasCustom);
            if (hasCustom)
            {
                foreach (var customMapId in ModsManager.Instance.CustomMapIds)
                {
                    MapConfigData config = SaveUtility.Load<MapConfigData>(Path.Combine(CCConstants.MapsDir, customMapId, "config.json"));
                    customMapOS.Options.Add(new CustomMapOption()
                    {
                        Id = $"{customMapId}#{config.Name}",
                    });
                }
                customMapOS.Select(0, false);
            }
            multiplayerMenu.OnOpen += UpdateMap;

            modeOS.SetupUsingEnum<Mode>(Mode.Timed);
            modeOS.Select(Mode.Adventure, false);
            
            visibilityOS.SetupUsingEnum<Visibility>();
            visibilityOS.OnSelected.AddListener(delegate (int option)
            {
                bool show = (Visibility)option == Visibility.Public;
                passwordCG.SetEnabled(show);
            });
            visibilityOS.Select(Visibility.Public);

            npcToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                pveCG.SetEnabled(isOn);
            });

            if (!PremiumManager.Data.IsPremium)
            {
                maxPlayersSlider.value = 4;
            }
        }

        private void OnClientDisconnect(ulong clientID)
        {
            UpdateStatus(LocalizationUtility.Localize("network_status_connection-failed"), Color.red);
            IsConnecting = false;
        }
        private void OnClientConnect(ulong clientID)
        {
            UpdateStatus(LocalizationUtility.Localize("network_status_connected"), Color.green);
        }

        public async void Join(string id)
        {
            if (!IsConnectedToInternet || !IsValidPlayer)
            {
                return;
            }
            IsConnecting = true;

            try
            {
                // Authenticate
                await Authenticate();

                // Check Unlocked Map
                WorldMP world = new WorldMP(await Lobbies.Instance.GetLobbyAsync(id));
                Map map = Enum.Parse<Map>(world.MapName);
                if (map == Map.ComingSoon)
                {
                    throw new Exception(LocalizationUtility.Localize("mainmenu_map-coming-soon"));
                }
                if ((world.Mode == Mode.Adventure) && !ProgressManager.Instance.IsMapUnlocked(map))
                {
                    throw new Exception(LocalizationUtility.Localize("mainmenu_map-locked", LocalizationUtility.Localize(world.MapId)));
                }

                // Check Version
                string version = world.Version;
                if (!version.Equals(Application.version))
                {
                    throw new Exception(LocalizationUtility.Localize("network_status_incorrect-version", Application.version, version));
                }

                // Check Password
                string passwordHash = world.PasswordHash;
                string password = "";
                if (!string.IsNullOrEmpty(passwordHash))
                {
                    password = await InputDialog.InputAsync(LocalizationUtility.Localize("mainmenu_multiplayer_password_title"), LocalizationUtility.Localize("mainmenu_multiplayer_password_placeholder"), error: LocalizationUtility.Localize("network_status_no-password-provided"));
                    bool isValidPasswordHash = string.IsNullOrEmpty(passwordHash) || sha256.VerifyHash(password, passwordHash);
                    if (!isValidPasswordHash)
                    {
                        throw new Exception(LocalizationUtility.Localize("network_status_invalid-password"));
                    }
                }

                // Check Kicked
                string playerId = AuthenticationService.Instance.PlayerId;
                if (world.KickedPlayers.Contains(playerId))
                {
                    throw new Exception(LocalizationUtility.Localize("network_status_kicked"));
                }

                // Check Mods
                if (world.IsCustom && !ModsManager.Instance.HasRequiredMods(world, out string reqMapId, out List<string> reqBodyPartIds, out List<string> reqPatternIds))
                {
                    int reqMods = (!string.IsNullOrEmpty(reqMapId) ? 1 : 0) + reqBodyPartIds.Count + reqPatternIds.Count;
                    ConfirmationDialog.Confirm(LocalizationUtility.Localize("mainmenu_multiplayer_custom_title"), LocalizationUtility.Localize("mainmenu_multiplayer_custom_message", reqMods), onYes: delegate
                    {
                        if (!string.IsNullOrEmpty(reqMapId))
                        {
                            ModsMenu.Instance.AddMod(reqMapId, FactoryItemType.Map);
                        }
                        foreach (var bodyPartId in reqBodyPartIds)
                        {
                            ModsMenu.Instance.AddMod(bodyPartId, FactoryItemType.BodyPart);
                        }
                        foreach (var patternId in reqPatternIds)
                        {
                            ModsMenu.Instance.AddMod(patternId, FactoryItemType.Pattern);
                        }
                        ModsMenu.Instance.Setup(() => InformationDialog.Inform(LocalizationUtility.Localize("mainmenu_mods_title"), LocalizationUtility.Localize("mainmenu_mods_done")));
                    });
                    throw new Exception(LocalizationUtility.Localize("network_status_mods-needed"));
                }

                // Set Connection Data
                string username = SettingsManager.Data.OnlineUsername;
                int level = ProgressManager.Data.Level;
                SetConnectionData(playerId, username, password, level);

                // Join Lobby
                UpdateStatus(LocalizationUtility.Localize("network_status_joining-lobby"), Color.yellow, -1);
                LobbyPlayer player = new LobbyPlayer(AuthenticationService.Instance.PlayerId);
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions()
                {
                    Player = player
                };
                world = new WorldMP(await LobbyHelper.Instance.JoinLobbyByIdAsync(id, options));

                if (EducationManager.Instance.IsEducational)
                {
                    UnityTransport unityTransport = NetworkTransportPicker.Instance.GetTransport<UnityTransport>("localarea");
                    NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
                    unityTransport.ConnectionData.Address = world.JoinCode;
                }
                else
                {
                    // Join Relay
                    UpdateStatus(LocalizationUtility.Localize("network_status_joining-via-relay"), Color.yellow, -1);
                    string joinCode = world.JoinCode;
                    JoinAllocation join = await Relay.Instance.JoinAllocationAsync(joinCode);

                    await Lobbies.Instance.UpdatePlayerAsync(world.Id, player.Id, new UpdatePlayerOptions()
                    {
                        AllocationId = join.AllocationId.ToString(),
                        ConnectionInfo = joinCode
                    });
                    UnityTransport unityTransport = NetworkTransportPicker.Instance.GetTransport<UnityTransport>("relay");
                    NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
                    unityTransport.SetClientRelayData(join.RelayServer.IpV4, (ushort)join.RelayServer.Port, join.AllocationIdBytes, join.Key, join.ConnectionData, join.HostConnectionData);
                }

                // Start Client
                UpdateStatus(LocalizationUtility.Localize("network_status_starting-client"), Color.yellow, -1);
                Play();
                NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                if (e is NullReferenceException || (e is LobbyServiceException && (e as LobbyServiceException).Reason == LobbyExceptionReason.LobbyConflict))
                {
                    UpdateStatus(LocalizationUtility.Localize("network_status_lobby-error"), Color.red); // TODO: Bug with Lobby returning NullReferenceException?
                }
                else
                {
                    UpdateStatus(e.Message, Color.red);
                }
                IsConnecting = false;
            }
        }
        public async void Create()
        {
            if (!IsConnectedToInternet || !IsValidPlayer || !IsValidWorldName)
            {
                return;
            }
            IsConnecting = true;

            try
            {
                // Setup World
                bool isPrivate = (Visibility)visibilityOS.SelectedIndex == Visibility.Private;
                bool usePassword = passwordToggle.isOn && !isPrivate && !string.IsNullOrEmpty(passwordInputField.text);
                string worldName = worldNameInputField.text;
                string mapId = mapOS.Selected.Id;
                string mapName = ((Map)mapOS.SelectedIndex).ToString();
                string version = Application.version;
                int maxPlayers = (int)maxPlayersSlider.value;
                bool enablePVP = pvpToggle.isOn;
                bool spawnNPC = npcToggle.isOn;
                bool enablePVE = pveToggle.isOn;
                bool allowProfanity = profanityToggle.isOn;
                Mode mode = (Mode)modeOS.SelectedIndex;
                Map map = (Map)mapOS.SelectedIndex;
                int spawnPoint = spawnPointOS.SelectedIndex;
                string hostPlayerId = AuthenticationService.Instance.PlayerId;
                string kickedPlayers = "";
                string institutionId = EducationManager.Instance.InstitutionId;

                // Custom Map, Body Parts and Patterns
                string customMapId = "";
                string customBodyPartIds = "";
                string customPatternIds = "";
                if (map == Map.Custom)
                {
                    customMapId = ((CustomMapOption)customMapOS.Selected).MapId;

                    MapConfigData config = SaveUtility.Load<MapConfigData>(Path.Combine(CCConstants.MapsDir, customMapId, "config.json"));

                    customBodyPartIds = string.Join(",", config.BodyPartIds);
                    customPatternIds = string.Join(",", config.PatternIds);
                }

                // Check Premium
                if (!PremiumManager.Data.IsPremium)
                {
                    if (maxPlayers > 4)
                    {
                        maxPlayersSlider.value = 4;
                        throw new Exception(LocalizationUtility.Localize("mainmenu_premium_max-players"));
                    }

                    if (isPrivate)
                    {
                        visibilityOS.Select(Visibility.Public);
                        throw new Exception(LocalizationUtility.Localize("mainmenu_premium_private"));
                    }
                }

                // Check Map
                if (map == Map.ComingSoon)
                {
                    throw new Exception(LocalizationUtility.Localize("mainmenu_map-coming-soon"));
                }
                if ((mode == Mode.Adventure) && !ProgressManager.Instance.IsMapUnlocked(map))
                {
                    throw new Exception(LocalizationUtility.Localize("mainmenu_map-locked", LocalizationUtility.Localize(mapId)));
                }
                
                // Set Connection Data
                string username = SettingsManager.Data.OnlineUsername;
                string password = NetworkHostManager.Instance.Password = (usePassword ? passwordInputField.text : "");
                string passwordHash = usePassword ? sha256.GetHash(password) : "";
                int level = ProgressManager.Data.Level;
                SetConnectionData(hostPlayerId.ToString(), username, password, level);
                
                // Authenticate
                await Authenticate();

                // Check Version
                await RemoteConfigUtility.FetchConfigAsync();
                var v1 = VersionUtility.GetVersion(RemoteConfigService.Instance.appConfig.GetString("min_online_version"));
                var v2 = VersionUtility.GetVersion(Application.version);
                if (v1.CompareTo(v2) > 0)
                {
                    throw new Exception(LocalizationUtility.Localize("network_status_outdated-version"));
                }

                string joinCode = null;
                string allocationId = null;
                if (EducationManager.Instance.IsEducational)
                {
                    UnityTransport unityTransport = NetworkTransportPicker.Instance.GetTransport<UnityTransport>("localarea");
                    NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
                    unityTransport.ConnectionData.Address = joinCode = NetworkUtils.GetLocalAddressIPv4();
                }
                else
                {
                    // Allocate Relay
                    UpdateStatus(LocalizationUtility.Localize("network_status_allocating-relay"), Color.yellow, -1);
                    Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxPlayers);
                    UnityTransport unityTransport = NetworkTransportPicker.Instance.GetTransport<UnityTransport>("relay");
                    NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
                    unityTransport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

                    // Generate Join Code
                    UpdateStatus(LocalizationUtility.Localize("network_status_generating-join-code"), Color.yellow, -1);
                    joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
                    allocationId = allocation.AllocationId.ToString();
                }

                // Create Lobby
                UpdateStatus(LocalizationUtility.Localize("network_status_creating-lobby"), Color.yellow, -1);
                CreateLobbyOptions options = new CreateLobbyOptions()
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>()
                    {
                        { "isPrivate", new DataObject(DataObject.VisibilityOptions.Public, isPrivate.ToString())},
                        { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
                        { "version", new DataObject(DataObject.VisibilityOptions.Public, version) },
                        { "mapName", new DataObject(DataObject.VisibilityOptions.Public, mapName) },
                        { "mapId", new DataObject(DataObject.VisibilityOptions.Public, mapId) },
                        { "passwordHash", new DataObject(DataObject.VisibilityOptions.Public, passwordHash) },
                        { "enablePVP", new DataObject(DataObject.VisibilityOptions.Public, enablePVP.ToString()) },
                        { "enablePVE", new DataObject(DataObject.VisibilityOptions.Public, enablePVE.ToString()) },
                        { "spawnNPC", new DataObject(DataObject.VisibilityOptions.Public, spawnNPC.ToString()) },
                        { "allowProfanity", new DataObject(DataObject.VisibilityOptions.Public, allowProfanity.ToString()) },
                        { "mode", new DataObject(DataObject.VisibilityOptions.Public, ((int)mode).ToString()) },
                        { "spawnPoint", new DataObject(DataObject.VisibilityOptions.Public, spawnPoint.ToString()) },
                        { "hostPlayerId", new DataObject(DataObject.VisibilityOptions.Public, hostPlayerId) },
                        { "kickedPlayers", new DataObject(DataObject.VisibilityOptions.Public, kickedPlayers) },
                        { "institutionId", new DataObject(DataObject.VisibilityOptions.Public, institutionId) },
                        { "customMapId", new DataObject(DataObject.VisibilityOptions.Public, customMapId) },
                        { "customBodyPartIds", new DataObject(DataObject.VisibilityOptions.Public, customBodyPartIds) },
                        { "customPatternIds", new DataObject(DataObject.VisibilityOptions.Public, customPatternIds) }
                    },
                    Player = new LobbyPlayer(AuthenticationService.Instance.PlayerId, joinCode, null, allocationId)
                };
                await LobbyHelper.Instance.CreateLobbyAsync(worldName, maxPlayers, options);

                // Start Host
                UpdateStatus(LocalizationUtility.Localize("network_status_starting-host"), Color.yellow, -1);
                Play();
                NetworkManager.Singleton.StartHost();
            }
            catch (Exception e)
            {
                UpdateStatus(e.Message, Color.red);
                IsConnecting = false;
            }
        }
        public async Task<int> Refresh()
        {
            await Authenticate();

            IsRefreshing = true;
            refreshCount++;

            worldsRT.transform.DestroyChildren();
            noneGO.SetActive(false);
            refreshButton.interactable = false;

            try
            {
                List<Lobby> lobbies = (await Lobbies.Instance.QueryLobbiesAsync()).Results;
                foreach (Lobby lobby in lobbies)
                {
                    WorldMP world = new WorldMP(lobby);

                    if (EducationManager.Instance.IsEducational && world.InstitutionId != EducationManager.Instance.InstitutionId)
                    {
                        continue;
                    }

                    if (!world.IsPrivate)
                    {
                        var v1 = VersionUtility.GetVersion(world.Version);
                        var v2 = VersionUtility.GetVersion(Application.version);

                        if (v1.CompareTo(v2) >= 0)
                        {
                            Instantiate(worldUIPrefab, worldsRT).Setup(this, lobby);
                        }
                    }
                }
                noneGO.SetActive(worldsRT.childCount == 0);
            }
            catch (Exception e)
            {
                UpdateStatus(e.Message, Color.red);
                noneGO.SetActive(true);
            }

            this.Invoke(delegate
            {
                refreshButton.interactable = true;
            }, 1f);
            IsRefreshing = false;

            return worldsRT.childCount;
        }
        public async void TryRefresh()
        {
            if (multiplayerMenu.IsOpen && !IsRefreshing && multiplayerSSS.SelectedPanel == 0 && IsConnectedToInternet)
            {
                int numWorlds = await Refresh();
                if (numWorlds == 0 && refreshCount == 2)
                {
                    multiplayerHintMenu.Open();
                }
            }
        }
        public void Cancel()
        {
            if (!IsConnecting) return;
            if (updateStatusCoroutine != null) StopCoroutine(updateStatusCoroutine);
            NetworkShutdownManager.Instance.Shutdown();
            HideStatus();
            IsConnecting = false;
        }
        public void Join()
        {
            Join(lobbyCodeInputField.text);
        }
        public void Play()
        {
            WorldManager.Instance.World = new WorldMP(LobbyHelper.Instance.JoinedLobby);
        }

        public void SortBy()
        {
            isSortedByAscending = !isSortedByAscending;

            List<WorldUI> worlds = new List<WorldUI>(worldsRT.GetComponentsInChildren<WorldUI>());
            worlds.Sort((x, y) => x.Players.CompareTo(y.Players));

            for (int i = 0; i < worlds.Count; ++i)
            {
                int siblingIndex = isSortedByAscending ? i : (worlds.Count - 1) - i;
                worlds[i].transform.SetSiblingIndex(siblingIndex);
            }
            sortByIcon.transform.localScale = new Vector3(1f, isSortedByAscending ? 1f : -1f, 1f);
        }
        public void Filter()
        {
            InputDialog.Input(LocalizationUtility.Localize("mainmenu_multiplayer_filter_title"), LocalizationUtility.Localize("mainmenu_multiplayer_filter_placeholder"), onSubmit: delegate (string filterText)
            {
                filterText = filterText.ToLower();

                foreach (Transform world in worldsRT)
                {
                    bool filtered = false;
                    if (!string.IsNullOrEmpty(filterText))
                    {
                        filtered = true;
                        foreach (TextMeshProUGUI tmp in world.GetComponentsInChildren<TextMeshProUGUI>())
                        {
                            string text = tmp.text.ToLower();
                            if (text.Contains(filterText.ToLower()))
                            {
                                filtered = false;
                                break;
                            }
                        }
                    }
                    world.gameObject.SetActive(!filtered);
                }
            });
        }

        private async Task Authenticate()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                UpdateStatus(LocalizationUtility.Localize("network_status_authenticating"), Color.yellow, -1);
                await AccountManager.Instance.SignInAsync("");
                HideStatus();
            }
        }
        private void SetConnectionData(string playerId, string username, string password, int level)
        {
            ConnectionData data = new ConnectionData(playerId, username, password, level);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        }
        
        private void UpdateMap()
        {
            mapUI.Setup(mapOS, modeOS, customMapOS);
        }
        private void UpdateStatus(string status, Color color, float duration = 5)
        {
            if (updateStatusCoroutine != null)
            {
                StopCoroutine(updateStatusCoroutine);
            }

            statusText.CrossFadeAlpha(0f, 0f, true);
            statusText.CrossFadeAlpha(1f, 0.25f, true);
            statusText.text = status;
            statusText.color = color;
            statusBT.IsBlinking = false;

            if (duration == -1)
            {
                statusBT.IsBlinking = true;
            }
            else
            {
                updateStatusCoroutine = this.Invoke(HideStatus, duration);
            }
        }
        private void HideStatus()
        {
            statusText.CrossFadeAlpha(0, 0.25f, true);
        }
        #endregion

        #region Enum
        public enum RelayServer
        {
            Unity,
            Steam
        }

        public enum Visibility
        {
            Public,
            Private
        }
        #endregion
    }
}