
using Steamworks;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DanielLochner.Assets.CreatureCreator;

public class CustomMapSceneLoader : MonoBehaviour
{
	public GameObject loading;
	public GameObject world;
	public GameObject error;
	public TMP_Text errorText;
	public TMP_Text downloadText;
	public Image downloadProgressImage;

/*	WorkshopItem pendingDownloadItem;
	float nextTimeToCheckInstallProgress;*/

	void Awake()
	{
		CustomMapLoader.OnCustomMapLoaded += OnCustomMapLoaded;
		CustomMapLoader.OnCustomMapLoadFailed += OnCustomMapLoadFailed;
	}

	void OnDestroy()
	{
		CustomMapLoader.OnCustomMapLoaded -= OnCustomMapLoaded;
		CustomMapLoader.OnCustomMapLoadFailed -= OnCustomMapLoadFailed;

/*
		if(pendingDownloadItem != null)
		{
			pendingDownloadItem.OnFirstScreenshotLoaded -= SetUpScreenshotBackground;
		}*/
	}

	void Start()
	{
		CustomMapLoader.Load($"{WorldManager.Instance.World.CustomMapPath}\\Map");

		/*if(MapManager.Instance.TryGetMapByID(MyceliumNetwork.GetLobbyData<string>(LobbyData.Map), out IMap map))
		{
			if(map.GetID().StartsWith(LocalMap.Prefix))
			{
				if(MyceliumNetwork.PlayerCount > 1)
				{
					OnCustomMapLoadFailed($"Cannot load local map with {MyceliumNetwork.PlayerCount} players.");
					return;
				}
				CustomMapLoader.Load(((LocalMap)map).GetLocalMapFolder());
			}
			else if(map.GetID().StartsWith(WorkshopMap.Prefix))
			{
				WorkshopMap workshopMap = (WorkshopMap)map;

				var fileId = workshopMap.GetFileID();

				float megabytes = workshopMap.Item.FileSize / Constants.BytesInAMegabyte;

				if(megabytes > Constants.MaxCustomMapSize)
				{
					OnCustomMapLoadFailed($"Map is too large. {megabytes.ToString("0.00")}mb exceeds the maximum {Constants.MaxCustomMapSize}mb.");
					return;
				}

				*//*	if(workshopMap.Item.FirstScreenshot != null)
					{
						SetUpScreenshotBackground(workshopMap.Item.FirstScreenshot);
					}

					workshopMap.Item.OnFirstScreenshotLoaded += SetUpScreenshotBackground;
	*//*
				SteamUGC.StartPlaytimeTracking(new PublishedFileId_t[] { fileId }, 1u);

				if(workshopMap.Item.DownloadItem())
				{
					workshopMap.Item.OnItemInstalled += OnItemInstalled;
					pendingDownloadItem = workshopMap.Item;
				}
				else
				{
					// I know theres a method in the WorkshopItem but fuck u I don't trust it
					LaunchInstalledWorkshopMap(fileId);
				}
				// I'm pretty sure this causes an issue if the map isn't fully downloaded?
				*//*Custom map load failed Failed to load Custom Map: path was null
					UnityEngine.Debug:Log(Object)
					CustomMapSceneLoader:OnCustomMapLoadFailed(String) (at Assets/_Scripts/CustomMapSceneLoader.cs:125)
					CustomMapLoader:Load(String) (at Assets/_Scripts/CustomMapLoader.cs:28)
					CustomMapSceneLoader:Start() (at Assets/_Scripts/CustomMapSceneLoader.cs:53)
					*//*
			}
		}
		else
		{
			OnCustomMapLoadFailed("Unable to parse chosen map");
			return;
		}*/
	}
/*
	void SetUpScreenshotBackground(Gif screenshot)
	{
		loadingBackground.texture = screenshot.Frames[0];
	}*/

	void OnItemInstalled()
	{
		/*pendingDownloadItem.OnItemInstalled -= OnItemInstalled;

		LaunchInstalledWorkshopMap(pendingDownloadItem.FileId);

		pendingDownloadItem = null;*/
	}

	void LaunchInstalledWorkshopMap(PublishedFileId_t fileId)
	{
		/*if(SteamUGC.GetItemInstallInfo(fileId, out ulong sizeOnDisk, out string folder, 2048u, out uint timeStamp))
		{
			downloadText.text = $"Loading Custom Map \"{(string.IsNullOrEmpty(pendingDownloadItem.Title) ? "Map Title Pending" : pendingDownloadItem.Title)}\"";

			CustomMapLoader.Load(folder);
		}
		else
		{
			OnCustomMapLoadFailed("Failed to download correctly.");
			return;
		}*/
	}

	void Update()
	{
		/*if(pendingDownloadItem == null)
			return;

		if(!pendingDownloadItem.Installed)
		{
			if(Time.realtimeSinceStartup > nextTimeToCheckInstallProgress)
			{
				// this is kinda expensive
				if(SteamUGC.GetItemDownloadInfo(pendingDownloadItem.FileId, out ulong bytesDownloaded, out ulong bytesTotal))
				{
					float megabyte = 1000000;

					string title = string.IsNullOrEmpty(pendingDownloadItem.Title) ? "Map Title Pending" : pendingDownloadItem.Title;
					string author = string.IsNullOrEmpty(pendingDownloadItem.Author) ? "Unkown" : pendingDownloadItem.Author;

					string text = $"Downloading Custom Map \"{title}\" by {author}";

					if(bytesDownloaded == 0)
					{
						downloadText.text = $"{text} (Preparing Download...)";
					}
					else
					{
						downloadProgressImage.fillAmount = (float)bytesDownloaded / (float)bytesTotal;

						float megabytesDownloaded = (float)bytesDownloaded / megabyte;
						float megabytesTotal = (float)bytesTotal / megabyte;

						downloadText.text = $"{text} ({megabytesDownloaded.ToString("0.00")}/{megabytesTotal.ToString("0.00")} mb)";
					}
				}

				nextTimeToCheckInstallProgress = Time.realtimeSinceStartup + 0.2f;
			}
		}*/
	}

	void OnCustomMapLoaded(Scene scene)
	{
        foreach (var resolver in GetComponentsInChildren<CustomMapLoadingPass>())
        {
            resolver.Load(scene);
        }

        loading.SetActive(false);
        world.SetActive(true);
    }

    void OnCustomMapLoadFailed(string reason)
	{
		Debug.Log("Custom map load failed " + reason);

		loading.SetActive(false);
		error.SetActive(true);

		errorText.text = $"Custom map load failed: {reason}";
	}
}