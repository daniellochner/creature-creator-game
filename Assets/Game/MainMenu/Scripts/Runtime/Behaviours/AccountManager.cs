using Crosstales.BWF;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DanielLochner.Assets.CreatureCreator
{
    public class AccountManager : MonoBehaviourSingleton<AccountManager>
    {
        #region Fields
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI placeholderText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Image refreshImg;
        [SerializeField] private Image identityProviderImg;
        [SerializeField] private GameObject errorGO;
        [SerializeField] private GameObject noErrorGO;
        [Space]
        [SerializeField] private Sprite anonymousIcon;
        [SerializeField] private Sprite unityIcon;
        [Space]
        [SerializeField] private GameObject[] signInSettings;
        [SerializeField] private GameObject[] signOutSettings;

        private string errorMessage;
        #endregion

        #region Properties
        private bool HasCheckedUsername
        {
            get => PlayerPrefs.GetInt("HAS_CHECKED_USERNAME", 0) == 1;
            set => PlayerPrefs.SetInt("HAS_CHECKED_USERNAME", value ? 1 : 0);
        }

        private string AccessToken
        {
            get => PlayerPrefs.GetString("ACCESS_TOKEN");
            set => PlayerPrefs.SetString("ACCESS_TOKEN", value);
        }
        #endregion

        #region Methods
        private void Start()
        {
            if (!SettingsManager.Instance.ShowTutorial)
            {
                Setup();
            }
        }

        public async void Setup()
        {
            if (NetworkUtils.IsConnectedToInternet)
            {
                string username = SettingsManager.Data.OnlineUsername;
                if (!HasCheckedUsername && !string.IsNullOrEmpty(username))
                {
                    await SignInAsync(username);
                }
                else
                {
                    await SignInAsync("");
                }
                HasCheckedUsername = true;
            }
            else
            {
                SetError(LocalizationUtility.Localize("account_status_no-internet"), true);
            }

            OnSetup();
        }

        public async void SignIn()
        {
            await SignInAsync("");
        }
        public async Task SignInAsync(string username)
        {
            SetError(null);
            SetRefreshing(true, "account_status_signing-in");

            try
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    if (string.IsNullOrEmpty(AccessToken))
                    {
                        await SignInAnonymouslyAsync();
                    }
                    else
                    {
                        await SignInWithUnityAsync();
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(AccessToken))
                    {
                        OnSignedInAnonymously();
                    }
                    else
                    {
                        OnSignedInWithUnity();
                    }
                }

                if (!string.IsNullOrEmpty(username))
                {
                    SetUsername(await AuthenticationService.Instance.UpdatePlayerNameAsync(ParseUsername(username)));
                }
                else
                if (!string.IsNullOrEmpty(SettingsManager.Data.OnlineUsername))
                {
                    SetUsername(await AuthenticationService.Instance.GetPlayerNameAsync());
                }
            }
            catch (AuthenticationException ex) // AuthenticationErrorCodes
            {
                Debug.LogException(ex);
                SetError(ex.Message);
            }
            catch (RequestFailedException ex) // CommonErrorCodes
            {
                Debug.LogException(ex);
                SetError(ex.Message);
            }

            SetRefreshing(false);
        }
        public async Task SignInAnonymouslyAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                OnSignedInAnonymously();
            }
            catch (AuthenticationException ex)
            {
                Debug.LogException(ex);
                SetError(ex.Message);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
                SetError(ex.Message);
            }
        }
        public async Task SignInWithUnityAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUnityAsync(AccessToken);

                OnSignedInWithUnity();
            }
            catch (AuthenticationException ex)
            {
                AccessToken = null;

                await SignInAnonymouslyAsync();

                Debug.LogException(ex);
                SetError(ex.Message);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
                SetError(ex.Message);
            }
        }
        public async Task LinkWithUnityAsync()
        {
            SetError(null);
            SetRefreshing(true, "account_status_linking");

            try
            {
                await PlayerAccountService.Instance.StartSignInAsync();
                await UniTask.WaitUntil(() => PlayerAccountService.Instance.IsSignedIn); // Wait until player is signed in with Unity account

                AccessToken = PlayerAccountService.Instance.AccessToken;

                await AuthenticationService.Instance.LinkWithUnityAsync(AccessToken);

                OnLinkedWithUnity();
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                // Replace Anonymous account with Unity account

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    AuthenticationService.Instance.SignOut(); // Sign out of Anonymous account

                    OnSignedOutOfAnonymous();
                }
                await SignInWithUnityAsync();

                OnReplacedWithUnity();
            }
            catch (AuthenticationException ex)
            {
                Debug.LogException(ex);
                SetError(ex.Message);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
                SetError(ex.Message);
            }

            SetRefreshing(false);
        }
        public async void UpdateUsername(string username)
        {
            SetError(null);
            SetRefreshing(true, "account_status_updating");

            try
            {
                SetUsername(await AuthenticationService.Instance.UpdatePlayerNameAsync(ParseUsername(username)));
            }
            catch (AuthenticationException ex)
            {
                Debug.LogException(ex);
                SetError(ex.Message);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
                SetError(ex.Message);
            }

            SetRefreshing(false);
        }

        public async void TrySignInWithUnity()
        {
            if (!NetworkUtils.IsConnectedToInternet)
            {
                InformationDialog.Inform(LocalizationUtility.Localize("account_status_no-internet"), LocalizationUtility.Localize("network_status_internet"));
                return;
            }

            if (AuthenticationService.Instance.IsSignedIn)
            {
                await LinkWithUnityAsync();
            }
        }
        public void TrySignOutOfUnity()
        {
            AccessToken = null;

            PlayerAccountService.Instance.SignOut();

            OnSignedOutOfUnity();
        }
        public void TryDeleteAccount()
        {
            Application.OpenURL("https://player-account.unity.com/delete-account");
            TrySignOutOfUnity();
        }
        public void TryUpdateUsername()
        {
            if (!NetworkUtils.IsConnectedToInternet)
            {
                InformationDialog.Inform(LocalizationUtility.Localize("account_status_no-internet"), LocalizationUtility.Localize("network_status_internet"));
                return;
            }

            if (AuthenticationService.Instance.IsSignedIn)
            {
                InputDialog.Input(LocalizationUtility.Localize("mainmenu_username_title"), LocalizationUtility.Localize("account_status_enter-a-username"), onSubmit: delegate (string username)
                {
                    var profanity = BWFManager.Instance.GetAll(username);
                    if (profanity.Count > 0)
                    {
                        InformationDialog.Inform(LocalizationUtility.Localize("profanity_detected_title"), LocalizationUtility.Localize("profanity_detected_message_terms", profanity.JoinAnd()));
                    }
                    else
                    {
                        UpdateUsername(username);
                    }
                });
            }
            else
            if (errorMessage != null)
            {
                InformationDialog.Inform(LocalizationUtility.Localize("account_status_error"), errorMessage, onOkay: SignIn);
            }
        }
        public void TrySyncData()
        {
            //if (!PremiumManager.Data.IsPremium)
            //{
            //    InformationDialog.Inform(LocalizationUtility.Localize("account_sync-data_title"), LocalizationUtility.Localize("account_sync-data_message"));
            //    return;
            //}

            // TODO: Implement cloud save for creatures/progress...
        }

        private void SetError(string message = null, bool setTitle = false)
        {
            bool isError = message != null;

            if (!isError || errorMessage == null)
            {
                errorMessage = message;
            }

            if (setTitle)
            {
                errorText.text = message;
            }
            else
            {
                errorText.text = LocalizationUtility.Localize("account_status_error");
            }

            errorGO.SetActive(isError);
            noErrorGO.SetActive(!isError);

            if (isError)
            {
                SetUnitySettingsVisibility(false); // Can't be signed in if there is an error
            }
        }
        private void SetRefreshing(bool isRefreshing, string status = null)
        {
            identityProviderImg.gameObject.SetActive(!isRefreshing);
            refreshImg.gameObject.SetActive(isRefreshing);

            usernameText.transform.parent.gameObject.SetActive(!isRefreshing);
            statusText.gameObject.SetActive(isRefreshing);

            if (!string.IsNullOrEmpty(status))
            {
                statusText.text = LocalizationUtility.Localize(status);
            }
            else
            {
                statusText.text = null;
            }
        }
        private void SetUsername(string username)
        {
            usernameText.text = username;

            bool isEmpty = string.IsNullOrEmpty(username);
            usernameText.gameObject.SetActive(!isEmpty);
            placeholderText.gameObject.SetActive(isEmpty);

            if (!isEmpty && username.Contains("#"))
            {
                SettingsManager.Data.OnlineUsername = username.Substring(0, username.LastIndexOf("#"));
            }
        }
        private void SetUnitySettingsVisibility(bool isSignedIn)
        {
            foreach (GameObject setting in signInSettings)
            {
                setting.SetActive(!isSignedIn);
            }
            foreach (GameObject setting in signOutSettings)
            {
                setting.SetActive(isSignedIn);
            }
        }

        private void OnSetup()
        {
            FriendsMenu.Instance.Setup();
            LeaderboardsManager.Instance.Setup();
        }
        private void OnSignedInAnonymously()
        {
            SetUnitySettingsVisibility(false);
            identityProviderImg.sprite = anonymousIcon;
        }
        private void OnSignedInWithUnity()
        {
            SetUnitySettingsVisibility(true);
            identityProviderImg.sprite = unityIcon;
        }
        private void OnSignedOutOfAnonymous()
        {
            FriendsManager.Instance.Initialized = false;
        }
        private void OnSignedOutOfUnity()
        {
            SetUnitySettingsVisibility(false);
            identityProviderImg.sprite = anonymousIcon;

            FriendsManager.Instance.Initialized = false;
            FriendsMenu.Instance.Refresh();
        }
        private void OnLinkedWithUnity()
        {
            SetUnitySettingsVisibility(true);
            identityProviderImg.sprite = unityIcon;
        }
        private void OnReplacedWithUnity()
        {
            FriendsMenu.Instance.Refresh();
        }

        #region Helper
        private string ParseUsername(string username)
        {
            username = username.Replace(" ", "");

            if (string.IsNullOrEmpty(username))
            {
                username = "Player";
            }

            return username;
        }
        #endregion
        #endregion
    }
}