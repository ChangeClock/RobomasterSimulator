using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode.Transports.UTP;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameManager gameManager;
    
    [SerializeField] public GameObject[] prefabList;

    [SerializeField] private Transform[] redSpawnPoints;
    [SerializeField] private Transform[] blueSpawnPoints;

    void Start()
    {

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
        // gameManager.SpawnUpload(id + 1);

        // OnSpawn(robotID);
    }
}