using UnityEngine;
using System.IO;
using System.Collections;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;

#if UNITY_STANDALONE
using Steamworks;
#endif

namespace DanielLochner.Assets.CreatureCreator
{
    public class FactoryManager : DataManager<FactoryManager, FactoryData>
    {
        [Header("Factory")]
        public SecretKey steamKey;
        public SecretKey serverAddress;
        public int hoursToCache = 1;


        public List<string> LoadedCreatureNames { get; } = new();

        public bool IsDownloadingItem { get; private set; }
        public bool IsDownloadingUsername { get; private set; }

        public Action OnLoaded { get; set; }
        public Action<FactoryData.DownloadedItemData> OnItemDataDownloaded { get; set; }


        protected override void Start()
        {
            base.Start();
            LoadItems();
        }

        // View
        public void ViewWorkshop()
        {
            if (SystemUtility.IsDevice(DeviceType.Desktop) && !EducationManager.Instance.IsEducational)
            {
#if UNITY_STANDALONE
                SteamFriends.ActivateGameOverlayToWebPage($"steam://url/SteamWorkshopPage/{CCConstants.AppId}");
#endif
            }
            else if (SystemUtility.IsDevice(DeviceType.Handheld) || EducationManager.Instance.IsEducational)
            {
                string url = $"https://steamcommunity.com/app/{CCConstants.AppId}/workshop/";
                Application.OpenURL(url);
            }
        }
        public void ViewWorkshopItem(ulong id)
        {
            if (SystemUtility.IsDevice(DeviceType.Desktop) && !EducationManager.Instance.IsEducational)
            {
#if UNITY_STANDALONE
                SteamFriends.ActivateGameOverlayToWebPage($"steam://url/CommunityFilePage/{id}");
#endif
            }
            else if (SystemUtility.IsDevice(DeviceType.Handheld) || EducationManager.Instance.IsEducational)
            {
                string url = $"https://steamcommunity.com/sharedfiles/filedetails/?id={id}";
                Application.OpenURL(url);
            }
        }

        // Manage Item
        public void LikeItem(ulong id)
        {
            if (SystemUtility.IsDevice(DeviceType.Desktop) && !EducationManager.Instance.IsEducational)
            {
#if UNITY_STANDALONE
                PublishedFileId_t fileId = new (id);
                SteamUGC.SetUserItemVote(fileId, true);
#endif
            }

            if (!Data.LikedItems.Contains(id))
            {
                Data.LikedItems.Add(id);
            }
        }
        public void DislikeItem(ulong id)
        {
            if (SystemUtility.IsDevice(DeviceType.Desktop) && !EducationManager.Instance.IsEducational)
            {
#if UNITY_STANDALONE
                PublishedFileId_t fileId = new (id);
                SteamUGC.SetUserItemVote(fileId, false);
#endif
            }

            if (!Data.DislikedItems.Contains(id))
            {
                Data.DislikedItems.Add(id);
            }
        }
        public void SubscribeItem(ulong id)
        {
            if (SystemUtility.IsDevice(DeviceType.Desktop) && !EducationManager.Instance.IsEducational)
            {
#if UNITY_STANDALONE
                PublishedFileId_t fileId = new (id);
                SteamUGC.SubscribeItem(fileId);
#endif
            }

            if (!Data.SubscribedItems.Contains(id))
            {
                Data.SubscribedItems.Add(id);
            }
        }
        public void UnsubscribeItem(ulong id)
        {
            if (SystemUtility.IsDevice(DeviceType.Desktop) && !EducationManager.Instance.IsEducational)
            {
#if UNITY_STANDALONE
                PublishedFileId_t fileId = new (id);
                SteamUGC.UnsubscribeItem(fileId);
#endif
            }

            if (Data.SubscribedItems.Contains(id))
            {
                Data.SubscribedItems.Remove(id);
            }
        }
        public void RemoveItem(ulong id, FactoryItemType tag)
        {
            if (Data.DownloadedItems.ContainsKey(id))
            {
                string itemPath = Path.Combine(CCConstants.GetItemsDir(tag), id.ToString());
                if (Directory.Exists(itemPath))
                {
                    Directory.Delete(itemPath, true);
                }

                Data.DownloadedItems.Remove(id);
                Save();

                OnLoaded?.Invoke();
            }
        }

        // Get Items
        public void GetItems(FactoryItemQuery itemQuery, Action<List<FactoryItem>, uint> onLoaded, Action<string> onFailed)
        {
            if (!string.IsNullOrEmpty(itemQuery.SearchText))
            {
                itemQuery.SortByType = FactorySortByType.SearchText;
            }

            if (WorldTimeManager.Instance.IsInitialized)
            {
                var now = WorldTimeManager.Instance.UtcNow;
                if (Data.CachedItems.TryGetValue(itemQuery, out FactoryData.CachedItemData data))
                {
                    var time = new DateTime(data.Ticks);

                    TimeSpan diff = now - time;
                    if (diff.Hours > hoursToCache)
                    {
                        Data.CachedItems.Remove(itemQuery);
                    }
                    else
                    {
                        onLoaded(data.Items, data.Total);
                        return;
                    }
                }
            }


            uint days = 0;
            switch (itemQuery.TimePeriodType)
            {
                case FactoryTimePeriodType.Today:
                    days = 1;
                    break;

                case FactoryTimePeriodType.ThisWeek:
                    days = 7;
                    break;

                case FactoryTimePeriodType.ThisMonth:
                    days = 30;
                    break;

                case FactoryTimePeriodType.ThisYear:
                    days = 365;
                    break;

                case FactoryTimePeriodType.AllTime:
                    days = 9999999;
                    break;
            }

            if (SystemUtility.IsDevice(DeviceType.Desktop) && !EducationManager.Instance.IsEducational)
            {
#if UNITY_STANDALONE
                EUGCQuery sortBy = default;
                switch (itemQuery.SortByType)
                {
                    case FactorySortByType.MostPopular:
                        sortBy = EUGCQuery.k_EUGCQuery_RankedByTrend;
                        break;

                    case FactorySortByType.MostSubscribed:
                        sortBy = EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions;
                        break;

                    case FactorySortByType.MostRecent:
                        sortBy = EUGCQuery.k_EUGCQuery_RankedByPublicationDate;
                        break;

                    case FactorySortByType.LastUpdated:
                        sortBy = EUGCQuery.k_EUGCQuery_RankedByLastUpdatedDate;
                        break;

                    case FactorySortByType.SearchText:
                        sortBy = EUGCQuery.k_EUGCQuery_RankedByTextSearch;
                        break;
                }

                CallResult<SteamUGCQueryCompleted_t> query = CallResult<SteamUGCQueryCompleted_t>.Create(delegate (SteamUGCQueryCompleted_t param, bool hasFailed)
                {
                    if (hasFailed)
                    {
                        onFailed?.Invoke(null);
                        return;
                    }

                    List<FactoryItem> items = new ();
                    for (uint i = 0; i < param.m_unNumResultsReturned; i++)
                    {
                        FactoryItem item = new ()
                        {
                            tag = itemQuery.TagType
                        };
                        if (SteamUGC.GetQueryUGCResult(param.m_handle, i, out SteamUGCDetails_t details))
                        {
                            item.id = details.m_nPublishedFileId.m_PublishedFileId;
                            item.name = details.m_rgchTitle;
                            item.description = details.m_rgchDescription;
                            item.upVotes = details.m_unVotesUp;
                            item.timeCreated = details.m_rtimeCreated;
                            item.timeUpdated = details.m_rtimeUpdated;
                            item.creatorId = details.m_ulSteamIDOwner;
                        }
                        if (SteamUGC.GetQueryUGCPreviewURL(param.m_handle, i, out string url, 256))
                        {
                            item.previewURL = url;
                        }
                        items.Add(item);
                    }

                    uint total = param.m_unTotalMatchingResults;

                    onLoaded?.Invoke(items, total);

                    CacheItems(itemQuery, items, total);
                });

                UGCQueryHandle_t handle = SteamUGC.CreateQueryAllUGCRequest(sortBy, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, SteamUtils.GetAppID(), SteamUtils.GetAppID(), (uint)(itemQuery.Page + 1));
                SteamUGC.SetRankedByTrendDays(handle, days);
                SteamUGC.SetReturnLongDescription(handle, true);
                SteamUGC.SetSearchText(handle, itemQuery.SearchText);
                if (itemQuery.TagType == FactoryItemType.Creature)
                {
                    SteamUGC.AddExcludedTag(handle, FactoryItemType.Map.ToString());
                    SteamUGC.AddExcludedTag(handle, FactoryItemType.BodyPart.ToString());
                    SteamUGC.AddExcludedTag(handle, FactoryItemType.Pattern.ToString());
                }
                else
                {
                    SteamUGC.AddRequiredTag(handle, itemQuery.TagType.ToString());
                }

                SteamAPICall_t call = SteamUGC.SendQueryUGCRequest(handle);
                query.Set(call);
#endif
            }
            else if (SystemUtility.IsDevice(DeviceType.Handheld) || EducationManager.Instance.IsEducational)
            {
                string sortBy = default;
                switch (itemQuery.SortByType)
                {
                    case FactorySortByType.MostPopular:
                        sortBy = "3";
                        break;

                    case FactorySortByType.MostSubscribed:
                        sortBy = "9";
                        break;

                    case FactorySortByType.MostRecent:
                        sortBy = "1";
                        break;

                    case FactorySortByType.LastUpdated:
                        sortBy = "21";
                        break;

                    case FactorySortByType.SearchText:
                        sortBy = "12";
                        break;
                }

                string tags = "";
                if (itemQuery.TagType == FactoryItemType.Creature)
                {
                    tags = $"excludedtags[0]={FactoryItemType.Map}&excludedtags[1]={FactoryItemType.BodyPart}&excludedtags[2]={FactoryItemType.Pattern}";
                }
                else
                {
                    tags = $"requiredtags[0]={itemQuery.TagType}";
                }

                string url = $"https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/?key={steamKey.Value}&appid={CCConstants.AppId}&query_type={sortBy}&{tags}&search_text={itemQuery.SearchText}&days={days}&numperpage={itemQuery.NumPerPage}&page={itemQuery.Page + 1}&return_vote_data=true&return_previews=true";
                StartCoroutine(GetItemsRoutine(url, itemQuery, onLoaded, onFailed));
            }
        }
        public IEnumerator GetItemsRoutine(string url, FactoryItemQuery query, Action<List<FactoryItem>, uint> onLoaded, Action<string> onFailed)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                List<FactoryItem> items = new();

                JObject data = JToken.Parse(request.downloadHandler.text).First.First as JObject;
                uint total = data["total"].Value<uint>();

                if (total > 0)
                {
                    JArray files = data["publishedfiledetails"] as JArray;
                    foreach (JObject file in files)
                    {
                        string title = file["title"].Value<string>();
                        string description = file["file_description"].Value<string>();
                        ulong id = file["publishedfileid"].Value<ulong>();
                        ulong creatorId = file["creator"].Value<ulong>();
                        uint upVotes = file["vote_data"]["votes_up"].Value<uint>();
                        uint downVotes = file["vote_data"]["votes_down"].Value<uint>();
                        string previewURL = file["preview_url"].Value<string>();
                        uint timeCreated = file["time_created"].Value<uint>();
                        uint timeUpdated = file["time_updated"].Value<uint>();

                        FactoryItem item = new FactoryItem()
                        {
                            id = id,
                            name = title,
                            creatorId = creatorId,
                            description = description,
                            upVotes = upVotes,
                            downVotes = downVotes,
                            previewURL = previewURL,
                            timeCreated = timeCreated,
                            timeUpdated = timeUpdated,
                            tag = query.TagType
                        };
                        items.Add(item);
                    }
                }

                onLoaded?.Invoke(items, total);

                CacheItems(query, items, total);
            }
            else
            {
                onFailed?.Invoke(request.error);
            }
        }
        public void CacheItems(FactoryItemQuery query, List<FactoryItem> items, uint total)
        {
            if (!Data.CachedItems.ContainsKey(query))
            {
                Data.CachedItems.Add(query, new FactoryData.CachedItemData()
                {
                    Items = items,
                    Total = total
                });
                Save();
            }
        }

        // Get Item Data
        public void DownloadItemData(ulong itemId, Action<FactoryData.DownloadedItemData> onDownloaded = null, Action<string> onFailed = null)
        {
            StartCoroutine(DownloadItemDataRoutine(itemId, onDownloaded, onFailed));
        }
        public IEnumerator DownloadItemDataRoutine(ulong itemId, Action<FactoryData.DownloadedItemData> onDownloaded, Action<string> onFailed)
        {
            string url = $"https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/";

            WWWForm form = new WWWForm();
            form.AddField("itemcount", 1.ToString());
            form.AddField("publishedfileids[0]", itemId.ToString());

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    onFailed?.Invoke(www.error);
                }
                else
                {
                    JObject data = JToken.Parse(www.downloadHandler.text)["response"]["publishedfiledetails"].First as JObject;
                    string title = data["title"].Value<string>();
                    uint timeUpdated = data["time_updated"].Value<uint>();
                    List<string> tags = new List<string>();
                    JArray jTags = data["tags"].Value<JArray>();
                    if (jTags.HasValues)
                    {
                        foreach (var jTag in jTags)
                        {
                            tags.Add(jTag["tag"].Value<string>().ToLower());
                        }
                    }

                    FactoryItemType tag = FactoryItemType.Creature;
                    if (tags.Contains("map"))
                    {
                        tag = FactoryItemType.Map;
                    }
                    else
                    if (tags.Contains("bodypart"))
                    {
                        tag = FactoryItemType.BodyPart;
                    }
                    else
                    if (tags.Contains("pattern"))
                    {
                        tag = FactoryItemType.Pattern;
                    }

                    FactoryData.DownloadedItemData itemData = new FactoryData.DownloadedItemData()
                    {
                        Id = itemId,
                        Name = title,
                        Tag = tag,
                        Version = timeUpdated
                    };
                    onDownloaded?.Invoke(itemData);
                }
            }
        }

        // Download Item
        public void DownloadItem(FactoryItem item, Action<string> onDownloaded, Action<string> onFailed)
        {
            if (!IsDownloadingItem)
            {
#if UNITY_STANDALONE
                PublishedFileId_t fileId = new PublishedFileId_t(item.id);
                if (SteamUGC.DownloadItem(fileId, true))
                {
                    Callback<DownloadItemResult_t> callback = null;
                    callback = Callback<DownloadItemResult_t>.Create(delegate (DownloadItemResult_t param)
                    {
                        if (param.m_unAppID.m_AppId != CCConstants.AppId)
                        {
                            return;
                        }

                        string result = param.m_eResult.ToString();

                        if (param.m_eResult != EResult.k_EResultOK)
                        {
                            onFailed?.Invoke(result);
                            return;
                        }

                        onDownloaded?.Invoke(result);
                        OnItemDownloaded(item);

                        callback.Dispose();
                    });
                }
#else
                StartCoroutine(DownloadItemRoutine(item, onDownloaded, onFailed));
#endif
            }
        }
        public IEnumerator DownloadItemRoutine(FactoryItem item, Action<string> onDownloaded, Action<string> onFailed)
        {
            IsDownloadingItem = true;

            string url = $"http://{serverAddress.Value}/api/get_workshop_item?id={item.id}";

            string archiveName = $"{item.id}.zip";
            string archivePath = Path.Combine(Application.persistentDataPath, archiveName);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.downloadHandler = new DownloadHandlerFile(archivePath);
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    onFailed?.Invoke(webRequest.error);
                }
                else
                {
                    string extractPath = Path.Combine(CCConstants.GetItemsDir(item.tag), Path.GetFileNameWithoutExtension(archiveName));

                    if (Directory.Exists(extractPath))
                    {
                        Directory.Delete(extractPath, true); // Delete the directory if it exists to avoid conflicts
                    }

                    ZipFile.ExtractToDirectory(archivePath, extractPath, true);
                    File.Delete(archivePath);

                    onDownloaded?.Invoke(webRequest.result.ToString());
                    OnItemDownloaded(item);
                }
            }

            IsDownloadingItem = false;
        }
        private void OnItemDownloaded(FactoryItem item)
        {
            LoadItems(item.id);

            DownloadItemData(item.id, delegate (FactoryData.DownloadedItemData data)
            {
                if (Data.DownloadedItems.ContainsKey(item.id))
                {
                    Data.DownloadedItems[item.id] = data;
                }
                else
                {
                    Data.DownloadedItems.Add(item.id, data);
                }
                Save();
                OnItemDataDownloaded?.Invoke(data);
            },
            delegate (string reason)
            {
                Debug.Log(reason);
            });
        }

        // Download Username
        public void DownloadUsername(ulong userId, Action<string> onLoaded, Action<string> onFailed)
        {
            if (!IsDownloadingUsername)
            {
                StartCoroutine(DownloadUsernameRoutine(userId, onLoaded, onFailed));
            }
        }
        public IEnumerator DownloadUsernameRoutine(ulong userId, Action<string> onLoaded, Action<string> onFailed)
        {
            IsDownloadingUsername = true;

            string url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={steamKey.Value}&steamids={userId}";

            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject data = JToken.Parse(request.downloadHandler.text).First.First as JObject;

                JArray players = data["players"] as JArray;
                if (players.Count > 0)
                {
                    var player = players.First;

                    string username = player["personaname"].Value<string>();

                    onLoaded?.Invoke(username);

                    if (!Data.CachedUsernames.ContainsKey(userId))
                    {
                        Data.CachedUsernames.Add(userId, username);
                        Save();
                    }
                }
            }
            else
            {
                onFailed?.Invoke(request.error);
            }

            IsDownloadingUsername = false;
        }

        // Update Item
        public IEnumerator UpdateItemRoutine(ulong itemId, string title, string description, string dataPath, string previewPath, Action<float> onProgress, Action<string> onUploaded, Action<string> onFailed)
        {
#if UNITY_STANDALONE
            PublishedFileId_t fileId = new PublishedFileId_t(itemId);
            UGCUpdateHandle_t updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), fileId);
            SteamUGC.SetItemTitle(updateHandle, title);
            SteamUGC.SetItemDescription(updateHandle, description);
            SteamUGC.SetItemContent(updateHandle, dataPath);
            SteamUGC.SetItemPreview(updateHandle, previewPath);
            SteamUGC.SubmitItemUpdate(updateHandle, null);

            EItemUpdateStatus updateStatus = default;
            yield return new WaitUntil(() =>
            {
                updateStatus = SteamUGC.GetItemUpdateProgress(updateHandle, out ulong p, out ulong t);
                if (p >= 0 && t > 0)
                {
                    onProgress?.Invoke(p / (float)t);
                }
                else
                {
                    onProgress?.Invoke(0f);
                }
                return updateStatus == EItemUpdateStatus.k_EItemUpdateStatusInvalid;
            });

            if (true)
            {
                SteamFriends.ActivateGameOverlayToWebPage($"steam://url/CommunityFilePage/{itemId}");
                onUploaded?.Invoke(itemId.ToString());
            }
            else
            {
                onFailed?.Invoke("Error");
            }
#else
            yield return null;
#endif
        }

        // Upload Item
        public void UploadItem(string title, string description, FactoryItemType tagType, string dataPath, string previewPath, Action<float> onProgress = null, Action<string> onUploaded = null, Action<string> onFailed = null)
        {
            StartCoroutine(UploadItemRoutine(title, description, tagType, dataPath, previewPath, onProgress, onUploaded, onFailed));
        }
        public IEnumerator UploadItemRoutine(string title, string description, FactoryItemType tagType, string dataPath, string previewPath, Action<float> onProgress, Action<string> onUploaded, Action<string> onFailed)
        {
#if UNITY_STANDALONE
            UploadStatus uploadStatus = UploadStatus.Uploading;
            EItemUpdateStatus updateStatus = EItemUpdateStatus.k_EItemUpdateStatusUploadingContent;
            EResult error = default;

            PublishedFileId_t itemId = default;
            UGCUpdateHandle_t updateHandle = default;
            CallResult<CreateItemResult_t> item = CallResult<CreateItemResult_t>.Create(delegate (CreateItemResult_t createdItem, bool hasFailed)
            {
                if (createdItem.m_bUserNeedsToAcceptWorkshopLegalAgreement)
                {
                    SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/workshop/workshoplegalagreement/");
                }
                if (hasFailed)
                {
                    uploadStatus = UploadStatus.Error;
                    return;
                }

                itemId = createdItem.m_nPublishedFileId;
                updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), itemId);
                SteamUGC.SetItemTitle(updateHandle, title);
                SteamUGC.SetItemDescription(updateHandle, description);
                SteamUGC.SetItemContent(updateHandle, dataPath);
                SteamUGC.SetItemPreview(updateHandle, previewPath);
                SteamUGC.SetItemTags(updateHandle, new string[] { tagType.ToString() });
                SteamUGC.SetItemVisibility(updateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
                SteamUGC.SubmitItemUpdate(updateHandle, null);

                uploadStatus = UploadStatus.Uploaded;
                error = createdItem.m_eResult;
            });
            SteamAPICall_t createHandle = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
            item.Set(createHandle);

            yield return new WaitUntil(() => uploadStatus != UploadStatus.Uploading);

            if (uploadStatus == UploadStatus.Uploaded)
            {
                yield return new WaitUntil(() =>
                {
                    updateStatus = SteamUGC.GetItemUpdateProgress(updateHandle, out ulong p, out ulong t);
                    if (p >= 0 && t > 0)
                    {
                        onProgress?.Invoke(p / (float)t);
                    }
                    else
                    {
                        onProgress?.Invoke(0f);
                    }
                    return updateStatus == EItemUpdateStatus.k_EItemUpdateStatusInvalid;
                });

                SteamFriends.ActivateGameOverlayToWebPage($"steam://url/CommunityFilePage/{itemId}");
                onUploaded?.Invoke(itemId.ToString());
            }
            else
            {
                onFailed?.Invoke(error.ToString());
            }
#else
            yield return null;
#endif
        }
        public enum UploadStatus
        {
            Uploading,
            Error,
            Uploaded
        }

        // Load Items
        public void LoadItems(params ulong[] itemIds)
        {
            SystemUtility.TryCreateDirectory(CCConstants.CreaturesDir);
            SystemUtility.TryCreateDirectory(CCConstants.GetItemsDir(FactoryItemType.Creature));
            SystemUtility.TryCreateDirectory(CCConstants.MapsDir);
            SystemUtility.TryCreateDirectory(CCConstants.BodyPartsDir);
            SystemUtility.TryCreateDirectory(CCConstants.PatternsDir);

#if UNITY_STANDALONE
            uint n = SteamUGC.GetNumSubscribedItems();
            if (n > 0)
            {
                PublishedFileId_t[] items = new PublishedFileId_t[n];
                SteamUGC.GetSubscribedItems(items, n);

                foreach (PublishedFileId_t fileId in items)
                {
                    ulong itemId = fileId.m_PublishedFileId;
                    if (itemIds.Length > 0 && !itemIds.Contains(itemId))
                    {
                        continue;
                    }
                    if (SteamUGC.GetItemInstallInfo(fileId, out ulong sizeOnDisk, out string itemPath, 1024, out uint timeStamp) && TryGetItemType(itemPath, out FactoryItemType type))
                    {
                        string src = itemPath;
                        string dst = Path.Combine(CCConstants.GetItemsDir(type), itemId.ToString());
                        SystemUtility.CopyDirectory(src, dst, true);
                    }
                }
            }
#endif

            LoadedCreatureNames.Clear();
            foreach (string itemPath in Directory.GetDirectories(CCConstants.GetItemsDir(FactoryItemType.Creature)))
            {
                string creaturePathSrc = Directory.GetFiles(itemPath)[0];
                string creaturePathDst = Path.Combine(CCConstants.CreaturesDir, Path.GetFileName(creaturePathSrc));
                SystemUtility.CopyFile(creaturePathSrc, creaturePathDst);
                string creatureName = Path.GetFileNameWithoutExtension(creaturePathSrc);
                LoadedCreatureNames.Add(creatureName);
            }

            OnLoaded?.Invoke();
        }
        public bool TryGetItemType(string path, out FactoryItemType type)
        {
            if (!Directory.Exists(path))
            {
                type = default;
                return false;
            }

            var files = Directory.GetFiles(path);
            if (files.Length == 1) // one data file for creature
            {
                type = FactoryItemType.Creature;
            }
            else
            {
                type = FactoryItemType.Map;
            }
            return true;
        }
    }
}