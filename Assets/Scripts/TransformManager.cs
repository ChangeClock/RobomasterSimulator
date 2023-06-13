using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode.Transports.UTP;

public class TransformManager : NetworkBehaviour
{
    [SerializeField] private GameObject Hero;
    [SerializeField] private GameObject Infantry;
    [SerializeField] private GameObject Sentry;

    [SerializeField] private GameManager gameManager;

    private string serverIP;

    void Start()
    {
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        GUILayout.Label("RoboMaster Simulator");
        GUILayout.Label("Version: " + Application.version);

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else if (!NetworkManager.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId))
        {
            StatusLabels();

            SpawnButtons();

            if(IsServer)
            {
                RefereeButtons();
            }
        } 
        else 
        {
            StatusLabels();

            if(IsServer)
            {
                RefereeButtons();
            }
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        serverIP = GUILayout.TextField(serverIP);
        if (GUILayout.Button("Client")) {
            if (serverIP == null) serverIP = "127.0.0.1";
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = serverIP;
            NetworkManager.Singleton.StartClient();
        }
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        // GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);

        GUILayout.Label("ClientID: " + NetworkManager.Singleton.LocalClientId);
        // NetworkSpawnManager spawnManager = new NetworkSpawnManager();
        // GUILayout.Label("PlayerObject: " + NetworkManager.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId));
        GUILayout.Label("R-Outpost: " + gameManager.RefereeControllerList[18].GetHP());
        GUILayout.Label("B-Outpost: " + gameManager.RefereeControllerList[38].GetHP());
    }

    void SpawnButtons()
    {
        if (GUILayout.Button("R1 - Hero"))
        {
            SpawnServerRpc(0,1);
        }

        if (GUILayout.Button("R3 - Infantry"))
        {
            SpawnServerRpc(1,3);
        }

        if (GUILayout.Button("R4 - Infantry"))
        {
            SpawnServerRpc(1,4);
        }

        if (GUILayout.Button("R5 - Infantry"))
        {
            SpawnServerRpc(1,5);
        }

        if (GUILayout.Button("R6 - Infantry"))
        {
            SpawnServerRpc(1,6);
        }

        if (GUILayout.Button("B1 - Hero"))
        {
            SpawnServerRpc(0,21);
        }

        if (GUILayout.Button("B3 - Infantry"))
        {
            SpawnServerRpc(1,23);
        }

        if (GUILayout.Button("B4 - Infantry"))
        {
            SpawnServerRpc(1,24);
        }

        if (GUILayout.Button("B5 - Infantry"))
        {
            SpawnServerRpc(1,25);
        }

        if (GUILayout.Button("B6 - Sentry"))
        {
            SpawnServerRpc(1,26);
        }
    }

    void RefereeButtons()
    {
        if (GUILayout.Button("Reset R-Outpost"))
        {
            gameManager.RefereeControllerList[18].SetHP(1500);
        }

        if (GUILayout.Button("Reset B-Outpost"))
        {
            gameManager.RefereeControllerList[38].SetHP(1500);
        } 
    }

    // void Spawn(int model, ulong clientId){
    //     // TMD 到底怎么直接访问那个networkmanager里面的prefabList啊
    //     GameObject[] prefabList = {Hero, Infantry};
    //     GameObject player = Instantiate(prefabList[model], Vector3.right * -110 + Vector3.up * 5, Quaternion.identity);
    //     player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    //     isSpawned = true;
    // }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnServerRpc(int model, int robotID, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log("Server Side? " + NetworkManager.Singleton.IsServer);
        Debug.Log("Client ID: " + clientId);
        
        GameObject[] prefabList = {Hero, Infantry, Sentry};
        GameObject player = Instantiate(prefabList[model], Vector3.right * -110 + Vector3.up * 5, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}