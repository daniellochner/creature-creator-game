using DanielLochner.Assets.CreatureCreator;
using DanielLochner.Assets;
using System.Text;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class FestivalManager : MonoBehaviour
{
    public TextAsset creaturePreset;
    public bool isFestival;
    [DrawIf("isFestival", true)] public float inactiveTime;

    private float timeLeft;
    private Vector3 prevMousePosition;
    private bool isResetting;


    private void Update()
    {
        if (!isFestival) return;

        if (Input.GetKeyDown(KeyCode.F1))
        {
            ResetWorld(Mode.Creative);
        }
        else
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ResetWorld(Mode.Adventure);
        }

        if (Input.anyKey || Input.mousePosition != prevMousePosition || Input.mouseScrollDelta != Vector2.zero)
        {
            timeLeft = inactiveTime;
            prevMousePosition = Input.mousePosition;
        }
        else
        {
            if (timeLeft < 0)
            {
                ResetWorld(Mode.Creative);
                timeLeft = Mathf.Infinity;
            }
            else
            {
                timeLeft -= Time.deltaTime;
            }
        }
    }

    private void ResetWorld(Mode mode)
    {
        if (isResetting) return;
        isResetting = true;

        Fader.FadeInOut(1f, delegate
        {
            NetworkShutdownManager.Instance.Shutdown();
            SceneManager.LoadScene("Temp");
            this.Invoke(delegate
            {
                LoadWorld(mode);
                isResetting = false;
            },
            1f);
        });
    }

    public void LoadWorld(Mode mode)
    {
        // Setup Data
        ProgressManager.Instance.Revert();
        SettingsManager.Data.CreaturePresets.Clear();
        if (mode == Mode.Creative)
        {
            var creature = JsonUtility.FromJson<CreatureData>(creaturePreset.text);
            SettingsManager.Data.CreaturePresets.Add(creature);
        }
        else
        {
            SettingsManager.Instance.SetTutorial(true);
        }

        // Setup World
        string mapName = "Island";
        bool spawnNPC = true;
        bool enablePVE = true;
        bool unlimited = false;
        WorldManager.Instance.World = new WorldSP(mapName, mode, spawnNPC, enablePVE, unlimited);

        // Set Connection Data
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = NetworkTransportPicker.Instance.GetTransport<UnityTransport>("localhost");
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new ConnectionData("", "", "", ProgressManager.Data.Level)));

        // Start Host
        NetworkManager.Singleton.StartHost();
    }
}
