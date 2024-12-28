using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

namespace DanielLochner.Assets
{
    public class PlayerChatManager : NetworkSingleton<PlayerChatManager>
    {
        public PlayerChatMenu chatMenu;
        public float messageCooldown = 1f;
        public int maxMessageLength = 256;
        public Color friendColor;
        public Keybind keybind;
        public bool checkForProfanity;

        private float lastMessageTime;

        public Action<string> OnMessageSent { get; set; }
        public Action<ulong, string> OnMessageReceived { get; set; }

        public bool CanSend => Time.time > (lastMessageTime + messageCooldown);

        public void Start()
        {
            Instantiate(chatMenu, Dynamic.OverlayCanvasSafe);
        }

        public bool TrySendChatMessage(string input)
        {
            if (TextSanitizer.TrySanitize(input, maxMessageLength, checkForProfanity, out string output) && CanSend)
            {
                ulong clientId = NetworkManager.Singleton.LocalClientId;

                OnMessageReceived(clientId, output);
                SendChatMessageServerRpc(clientId, output);
                OnMessageSent?.Invoke(output);

                lastMessageTime = Time.time;

                return true;
            }
            else
            {
                return false;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendChatMessageServerRpc(ulong clientId, string message)
        {
            SendChatMessageClientRpc(clientId, message, NetworkUtils.DontSendTo(clientId));
        }

        [ClientRpc]
        public void SendChatMessageClientRpc(ulong clientId, string message, ClientRpcParams clientRpcParams)
        {
            OnMessageReceived?.Invoke(clientId, message);
        }
    }
}