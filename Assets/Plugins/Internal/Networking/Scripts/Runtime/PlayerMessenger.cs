using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

namespace DanielLochner.Assets
{
    public class PlayerMessenger : NetworkBehaviour
    {
        #region Fields
        [SerializeField] private GameObject messagePrefab;
        [Space]
        [SerializeField] private bool checkForProfanity;
        [SerializeField] private int characterLimit;
        [SerializeField] private float sendCooldown;
        [SerializeField] protected float height;

        private float lastMessageTime;
        protected GameObject messageGO;
        #endregion

        #region Properties
        public bool CheckForProfanity
        {
            get => checkForProfanity;
            set => checkForProfanity = value;
        }

        public virtual bool CanSend => (Time.time > (lastMessageTime + sendCooldown));
        public virtual bool CanReceive => Camera.main != null;

        public virtual bool TryOpen => Input.GetKeyDown(KeyCode.T);
        #endregion

        #region Methods
        private void Update()
        {
            if (IsOwner)
            {
                HandleInput();
            }
        }
        private void OnDisable()
        {
            if (messageGO != null)
            {
                Destroy(messageGO);
            }
        }

        protected void HandleInput()
        {
            if (TryOpen)
            {
                Open();
            }
        }

        public void Open()
        {
            if (!InputDialog.Instance.IsOpen && CanSend)
            {
                InputDialog.Input(LocalizationUtility.Localize("send_message_title"), onSubmit: TrySendMessage, maxCharacters: characterLimit);
            }
        }

        public void TrySendMessage(string input)
        {
            if (TextSanitizer.TrySanitize(input, characterLimit, checkForProfanity, out string output) && CanSend)
            {
                SendMessageServerRpc(output);
                lastMessageTime = Time.time;
            }
        }
        protected virtual void ReceiveMessage(string message)
        {
            if (messageGO != null)
            {
                Destroy(messageGO);
            }
            messageGO = Instantiate(messagePrefab, transform.position + transform.up * height, transform.rotation, transform);

            messageGO.GetComponentInChildren<TextMeshProUGUI>().text = message;
            messageGO.GetComponent<LookAtConstraint>().AddSource(new ConstraintSource() { sourceTransform = Camera.main.transform, weight = 1f });

            PlayerChatManager.Instance.OnMessageReceived(OwnerClientId, message);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendMessageServerRpc(string message)
        {
            SendMessageClientRpc(message);
        }

        [ClientRpc]
        public void SendMessageClientRpc(string message)
        {
            if (CanReceive)
            {
                ReceiveMessage(message);
            }
        }
        #endregion
    }
}