using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode.Transports.UTP;

public class TransformManager : MonoBehaviour
{
    [SerializeField] private GameObject Hero;
    [SerializeField] private GameObject Infantry;
    [SerializeField] private GameObject Sentry;
    private NetworkPrefabsList prefabList;
    private string serverIP;

    private bool isSpawned = false;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        GUILayout.Label("RoboMaster Simulator");
        GUILayout.Label("Version: " + Application.version);

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else if (!isSpawned)
        {
            StatusLabels();

            SpawnButtons();
        } 
        else 
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        serverIP = GUILayout.TextField(serverIP);
        if (GUILayout.Button("Client") && serverIP != "") {
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = serverIP;
            NetworkManager.Singleton.StartClient();
        }
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    void SpawnButtons()
    {
        if (GUILayout.Button("Hero"))
        {
            if(NetworkManager.Singleton.IsServer)
            {
                Spawn(0);
            } 
            else 
            {
                SpawnServerRpc(0);
            }
        }

        if (GUILayout.Button("Infantry"))
        {
            if(NetworkManager.Singleton.IsServer)
            {
                Spawn(1);
            } 
            else 
            {
                SpawnServerRpc(1);
            }
        }
    }

    public void Spawn(int model)
    {
        // TMD 到底怎么直接访问那个networkmanager里面的prefabList啊
        GameObject[] prefabList = {Hero, Infantry};
        GameObject player = Instantiate(prefabList[model], Vector3.right * -110, Quaternion.identity);
        player.GetComponent<NetworkObject>().Spawn();
        isSpawned = true;
    }

    [ServerRpc]
    public void SpawnServerRpc(int model)
    {
        Spawn(model);
        isSpawned = true;
    }


}