using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode.Transports.UTP;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameManager gameManager;
    
    [SerializeField] public GameObject[] prefabList;

    void Start()
    {

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
                player = Instantiate(prefabList[id+1], Vector3.right * -110 + Vector3.up * 5, Quaternion.identity);
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