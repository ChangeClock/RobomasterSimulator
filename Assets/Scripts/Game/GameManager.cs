using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerStatus
{
    public int hp;
}

public class GameManager : NetworkBehaviour
{
    private List<PlayerStatus> PlayerStatusList = new List<PlayerStatus>();

    private void OnEnable()
    {
        // ArmorController.OnHit += DamageHandler;
    }

    private void OnDisable()
    {
        // ArmorController.OnHit -= DamageHandler;
    }

    public void DamageHandler()
    {
        if(NetworkManager.Singleton.IsServer){
            Debug.Log("Damage Occur On Server Side");
        }else{
            DamageEventServerRpc();
        }
    }
    
    [ServerRpc]
    public void DamageEventServerRpc()
    {
        Debug.Log("Damage Occur On Client Side, Recieved by Server");
    }
}
