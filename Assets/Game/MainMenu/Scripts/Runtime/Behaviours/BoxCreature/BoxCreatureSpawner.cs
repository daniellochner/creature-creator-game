// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace DanielLochner.Assets.CreatureCreator
{
    public class BoxCreatureSpawner : MonoBehaviour
    {
        #region Fields
        [SerializeField] private SecretKey creatureEncryptionKey;
        [Space]
        [SerializeField] private BoxCreature boxCreaturePrefab;
        [SerializeField] private Transform boxCreaturesRoot;
        [SerializeField] private Animator spawnerAnimator;
        [SerializeField] private AudioSource spawnerAudioSource;
        [SerializeField] private AudioClip prepareAudioClip;
        [SerializeField] private AudioClip spawnAudioClip;
        [Space]
        [SerializeField] private float spawnCooldown;
        [SerializeField] private float spawnStart;
        [SerializeField] private float rotationOffset;
        [SerializeField] private Vector3 force;

        private List<string> creatures = new List<string>();
        private int index;
        private Coroutine spawnCoroutine;
        #endregion

        #region Methods
        private void Start()
        {
            ChangeSpawnerSource(SpawnerSource.Local);
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(RectTransformUtility.ScreenPointToRay(Camera.main, Input.mousePosition), out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Ragdoll")))
            {
                Vector3 dir = (hitInfo.point - Camera.main.transform.position).normalized;
                hitInfo.rigidbody.AddForce((dir * force.z) + (Vector3.up * force.y), ForceMode.Impulse);
            }
        }

        public void ChangeSpawnerSource(SpawnerSource source)
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            spawnCoroutine = StartCoroutine(SpawnRoutine(source));
        }
        private IEnumerator SpawnRoutine(SpawnerSource source)
        {
            creatures.Clear();
            index = 0;

            switch (source)
            {
                case SpawnerSource.Local:
                    if (Directory.Exists(CCConstants.CreaturesDir))
                    {
                        string[] creaturePaths = Directory.GetFiles(CCConstants.CreaturesDir);
                        creaturePaths.Shuffle();
                        for (int i = 0; i < creaturePaths.Length && i < 10; i++)
                        {
                            CreatureData creatureData = SaveUtility.Load<CreatureData>(creaturePaths[i], creatureEncryptionKey.Value);
                            if (creatureData != null)
                            {
                                creatures.Add(JsonUtility.ToJson(creatureData));
                            }
                        }
                    }
                    break;

                case SpawnerSource.Presets:
                    if (SettingsManager.Data.CreaturePresets.Count > 0)
                    {
                        foreach (CreatureData creature in SettingsManager.Data.CreaturePresets)
                        {
                            creatures.Add(JsonUtility.ToJson(creature));
                        }
                    }
                    break;

                case SpawnerSource.YouTube:
                    string key = DatabaseManager.GetDatabaseEntry<SecretKey>("Keys", "GoogleAPI").Value;
                    string parentId = DatabaseManager.GetDatabaseEntry<SecretKey>("Keys", "PinnedCommentId").Value;

                    // Parent
                    UnityWebRequest getParent = UnityWebRequest.Get($"https://youtube.googleapis.com/youtube/v3/comments?part=snippet&id={parentId}&textFormat=plainText&key={key}");
                    yield return getParent.SendWebRequest();

                    JObject pData = JObject.Parse(getParent.downloadHandler.text);
                    JObject pItems = pData.GetValue("items").First as JObject;
                    JObject pSnippet = (pItems as JObject).GetValue("snippet") as JObject;

                    string pTextDisplay = pSnippet.GetValue("textDisplay").ToString();
                    string acKey = "acceptedCreatures:\n";
                    string acVal = pTextDisplay.Substring(pTextDisplay.IndexOf(acKey) + acKey.Length);
                    List<string> acIDs = new List<string>(acVal.Split('\n'));

                    // Replies
                    UnityWebRequest getReplies = UnityWebRequest.Get($"https://youtube.googleapis.com/youtube/v3/comments?part=snippet&parentId={parentId}&textFormat=plainText&maxResults=100&key={key}");
                    yield return getReplies.SendWebRequest();

                    JObject rData = JObject.Parse(getReplies.downloadHandler.text);
                    JToken rItems = rData.GetValue("items");
                    foreach (JToken rItem in rItems)
                    {
                        string id = (rItem as JObject).GetValue("id").ToString();
                        id = id.Substring(id.IndexOf('.') + 1);
                        if (acIDs.Contains(id))
                        {
                            JObject rSnippet = (rItem as JObject).GetValue("snippet") as JObject;

                            string rPublishedDate = rSnippet.GetValue("publishedAt").ToString();
                            string rUpdatedDate = rSnippet.GetValue("updatedAt").ToString();
                            if (rPublishedDate == rUpdatedDate)
                            {
                                creatures.Add(rSnippet.GetValue("textDisplay").ToString());
                            }
                        }
                    }
                    break;
            }
            
            if (creatures.Count > 0)
            {
                yield return new WaitForSeconds(spawnStart);
                creatures.Shuffle();
                while (true)
                {
                    spawnerAnimator.SetTrigger("Spawn");
                    yield return new WaitForSeconds(spawnCooldown);
                }
            }
        }

        public void PrepareEvent()
        {
            spawnerAudioSource.PlayOneShot(prepareAudioClip);
        }
        public void SpawnEvent()
        {
            spawnerAudioSource.PlayOneShot(spawnAudioClip);

            this.InvokeAtEndOfFrame(delegate
            {
                Quaternion rotation = boxCreaturesRoot.rotation * Quaternion.Euler(Random.Range(-rotationOffset, rotationOffset), Random.Range(-rotationOffset, rotationOffset), Random.Range(-rotationOffset, rotationOffset));
                BoxCreature boxCreature = Instantiate(boxCreaturePrefab, boxCreaturesRoot.position, rotation, boxCreaturesRoot);

                CreatureData creature = JsonUtility.FromJson<CreatureData>(creatures[index++ % creatures.Count]);
                boxCreature.Spawn(creature);
            });
        }
        #endregion

        #region Enum
        public enum SpawnerSource
        {
            Presets,
            Local,
            YouTube
        }
        #endregion
    }
}