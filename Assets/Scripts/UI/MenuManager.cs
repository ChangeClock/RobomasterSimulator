using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour {
    
    [SerializeField] private TMP_Dropdown SceneSelection;

    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject[] MenuList;

    // [SerializeField] private GameObject FirstLevelMenu;
    [SerializeField] private Button MultiplayerBtn;
    [SerializeField] private Button SettingBtn;
    [SerializeField] private Button ExitBtn;
    
    // [SerializeField] private GameObject MultiplayerMenu;
    [SerializeField] private Button HostBtn;
    [SerializeField] private Button ServerBtn;
    [SerializeField] private TMP_InputField ServerInfo;
    [SerializeField] private Button ClientBtn;
    [SerializeField] private Button ExitMultiplayerBtn;

    // Multiplayer Info
    [SerializeField] private GameObject MultiplayerInfo;
    [SerializeField] private TMP_Text ClientInfo;
    [SerializeField] private TMP_Text NetworkInfo;

    [SerializeField] private TMP_Text FrameInfo;

    [SerializeField] private TMP_Text VersionInfo;

    void Awake()
    {
        // Application.targetFrameRate = 60;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        
        VersionInfo.text = "Robomaster Simulater @ " + Application.version;
        FrameInfo.text = (int)(1f / Time.unscaledDeltaTime) + "FPS";

        // FirstLevelMenu
        MultiplayerBtn.onClick.AddListener(() => EnterMenu(1));

        // MultiplayerMenu
        ExitMultiplayerBtn.onClick.AddListener(() => EnterMenu(0));
        HostBtn.onClick.AddListener(() => StartMultiplayer(0));
        ServerBtn.onClick.AddListener(() => StartMultiplayer(1));
        ClientBtn.onClick.AddListener(() => StartMultiplayer(2));
    }

    void Update()
    {
        if(NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            ClientInfo.text = "ClientID: " + NetworkManager.Singleton.LocalClientId;    
            // NetworkInfo.text = "Ping: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.LocalClientId) + "ms";
        }
    }

    void EnterMenu(int index = 0)
    {
        for(int i = 0; i < MenuList.Length; i++)
        {
            if(i == index)
            {
                MenuList[i].SetActive(true);
            } else {
                MenuList[i].SetActive(false);
            }
        }
    }

    void StartMultiplayer(int mode = 0, string serverInfo = "127.0.0.1:12333")
    {
        // string[] res = serverInfo.Split(":");
        // foreach (string _res in res)
        // {
        //     Debug.Log(_res);
        // }
        // string serverIP = res[0];
        // string serverPort = res[1];

        switch(mode){
            case 0:
                NetworkManager.Singleton.StartHost();
                break;
            case 1:
                NetworkManager.Singleton.StartServer();
                break;
            case 2:
                // NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = serverIP;
                NetworkManager.Singleton.StartClient();
                break;
            default:
                Debug.LogError($"Unknown multiplayer mode {mode}");
                break;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            var status = NetworkManager.Singleton.SceneManager.LoadScene(SceneSelection.captionText.text, LoadSceneMode.Single);
            
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {SceneSelection.captionText.text} " +
                        $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
            
            hide();
        }

        if (NetworkManager.Singleton.IsClient)
        {
            MultiplayerInfo.SetActive(true);
            hide();
        }
    }

    public void hide()
    {
        MainMenu.SetActive(false);
    }

    public void show()
    {
        MainMenu.SetActive(true);
    }
}