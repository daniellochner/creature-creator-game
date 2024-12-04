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
    public bool isFestival;
    public float inactiveTime;

    private float timeLeft;
    private Vector3 prevMousePosition;
    private bool isResetting;


    private void Update()
    {
        if (!isFestival) return;

        if (Input.GetKeyDown(KeyCode.F1))
        {
            ResetWorld();
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
                ResetWorld();
                timeLeft = Mathf.Infinity;
            }
            else
            {
                timeLeft -= Time.deltaTime;
            }
        }
    }

    private void ResetWorld()
    {
        if (isResetting) return;
        isResetting = true;

        Fader.FadeInOut(1f, delegate
        {
            NetworkShutdownManager.Instance.Shutdown();
            SceneManager.LoadScene("Temp");
            this.Invoke(delegate
            {
                LoadWorld();
                isResetting = false;
            },
            1f);
        });
    }

    public void LoadWorld()
    {
        // Setup World
        string mapName = "Island";
        Mode mode = Mode.Creative;
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
