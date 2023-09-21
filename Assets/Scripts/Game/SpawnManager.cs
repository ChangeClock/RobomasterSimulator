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
        RefereeController referee;

        // role: 0-observer 1-red 2-blue 3-referee
        switch(role)
        {
            case 1:
                player = Instantiate(prefabList[id+1], Vector3.right * -110 + Vector3.up * 5, Quaternion.identity);
                referee = player.GetComponent<RefereeController>();
                referee.RobotID.Value = id + 1;
                referee.faction.Value = Faction.Red;
                break;
            case 2:
                player = Instantiate(prefabList[id+1], Vector3.right * -110 + Vector3.up * 5, Quaternion.identity);
                referee = player.GetComponent<RefereeController>();
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