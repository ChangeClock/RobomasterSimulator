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

    [SerializeField] public GameObject[] prefabList;

    [SerializeField] private Transform[] redSpawnPoints;
    [SerializeField] private Transform[] blueSpawnPoints;

    void Start()
    {
        for (int i = 0; i < RedClasses.Length; i ++)
        {
            int tempVar = i;
            RedClasses[i].onClick.AddListener(() => SpawnPlayer(1,tempVar));
        }

        for (int i = 0; i < BlueClasses.Length; i ++)
        {
            int tempVar = i;
            BlueClasses[i].onClick.AddListener(() => SpawnPlayer(2,tempVar));
        }

        for (int i = 0; i < Oberver.Length; i ++)
        {
            int tempVar = i;
            Oberver[i].onClick.AddListener(() => SpawnPlayer(0,tempVar));
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
        spawnManager.SpawnServerRpc(role, id);
        // SpawnServerRpc(role, id);

        gameObject.SetActive(false);
        // SurroundCamera.enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnServerRpc(int role, int id, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        GameObject player;
        RefereeController referee;

        // role: 0-observer 1-red 2-blue 3-referee
        switch(role)
        {
            case 1:
                player = Instantiate(prefabList[id+1], redSpawnPoints[id].position, redSpawnPoints[id].rotation);
                referee = player.GetComponent<RefereeController>();
                if (referee != null) referee.spawnPoint = redSpawnPoints[id];
                referee.RobotID.Value = id + 1;
                referee.faction.Value = Faction.Red;
                break;
            case 2:
                player = Instantiate(prefabList[id+1], blueSpawnPoints[id].position, blueSpawnPoints[id].rotation);
                referee = player.GetComponent<RefereeController>();
                if (referee != null) referee.spawnPoint = blueSpawnPoints[id];
                referee.RobotID.Value = id + 21;
                referee.faction.Value = Faction.Blue;
                break;
            case 0:
            case 3:
            default:
                player = Instantiate(prefabList[0], Vector3.right * -110 + Vector3.up * 5, Quaternion.identity);
                break;
        }

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}