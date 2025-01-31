using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;

namespace DanielLochner.Assets
{
    public class WorldTimeManager : MonoBehaviourSingleton<WorldTimeManager>
    {
        public SecretKey serverAddress;

        private long initializedUnixTime;
        private float initializedStartupTime;

        public bool IsInitialized => initializedUnixTime != 0;

        public DateTime UtcNow
        {
            get
            {
                float offset = Time.realtimeSinceStartup - initializedStartupTime;
                return DateTime.UnixEpoch.AddSeconds(initializedUnixTime + offset);
            }
        }

        private IEnumerator Start()
        {
            string url = $"http://{serverAddress.Value}/api/get_unix_time";

            UnityWebRequest webRequest = UnityWebRequest.Get(url);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                initializedUnixTime = long.Parse(webRequest.downloadHandler.text);
                initializedStartupTime = Time.realtimeSinceStartup;
            }
            else
            {
                Debug.LogWarning(webRequest.error);
            }
        }
    }
}