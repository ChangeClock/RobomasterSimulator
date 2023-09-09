using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClassSelectionMenu : MonoBehaviour 
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private Camera SurroundCamera;

    // ClassSelection Menu
    [SerializeField] private Button[] RedClasses;
    [SerializeField] private Button[] BlueClasses;
    [SerializeField] private Button[] RefereeClasses;
    [SerializeField] private Button[] Oberver;

    // [SerializeField] public NetworkPrefabsList[] prefabList;
    [SerializeField] public GameObject[] prefabList;

    void Start()
    {
        for (int i = 0; i < RedClasses.Length; i ++)
        {
            int tempVar = i;
            RedClasses[i].onClick.AddListener(() => SpawnPlayer(1,tempVar));
        }
        // foreach(var (item, index) in Oberver.WithIndex())
        // {
        //     item.onClick.AddListener(() => SpawnPlayer(0,index));
        // }

        // foreach(var (item, index) in RedClasses.WithIndex())
        // {
        //     item.onClick.AddListener(() => SpawnPlayer(1,index));
        // }

        // foreach(var (item, index) in BlueClasses.WithIndex())
        // {
        //     item.onClick.AddListener(() => SpawnPlayer(2,index));
        // }

        // foreach(var (item, index) in RefereeClasses.WithIndex())
        // {
        //     item.onClick.AddListener(() => SpawnPlayer(3,index));
        // }
    }

    void Update()
    {

    }

    void SpawnPlayer(int role = 0, int id = 0)
    {
        // role: 0-observer 1-red 2-blue 3-referee

        // switch(role)
        // {
        //     case 0:
        //     case 3:

        //         break;
        //     case 1:
        //     case 2:
        //         SpawnServerRpc(role, id);
        //         break;
        // }

        spawnManager.SpawnServerRpc(role, id);

        gameObject.SetActive(false);
        SurroundCamera.enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnServerRpc(int role, int id, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        GameObject player = new GameObject();

        // role: 0-observer 1-red 2-blue 3-referee
        switch(role)
        {
            case 1:
            case 2:
                player = Instantiate(prefabList[id], Vector3.right * -110 + Vector3.up * 5, Quaternion.identity);
                RefereeController referee = player.GetComponent<RefereeController>();
                referee.RobotID = id + 1;
                break;
            case 0:
            case 3:
            default:
                player = Instantiate(prefabList[0], Vector3.right * -110 + Vector3.up * 5, Quaternion.identity);
                break;
        }

        // Debug.Log("Server Side? " + NetworkManager.Singleton.IsServer);
        // Debug.Log("Client ID: " + clientId);

        // TODO: Instantiate these players according to their robotID in fixed position
        // TODO: Instantiate these players according to their choice in a limited area

        // if (gameManager.RefereeControllerList[referee.RobotID] != null)
        // {
        //     Debug.LogError($"{referee.RobotID} exists !!!");
        //     return;
        // }
        
        // gameManager.RefereeControllerList.Add(referee.RobotID, referee);
        
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        // OnSpawn(robotID);
    }
}