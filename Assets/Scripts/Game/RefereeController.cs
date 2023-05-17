using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RefereeController : NetworkBehaviour
{
    [Header("Referee")]
    private ArmorController[] Armors;
    public int RobotID;
    public float[,] Status;

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnEnable()
    {
        Armors = this.gameObject.GetComponentsInChildren<ArmorController>();
        Debug.Log("Enable Armors: "+ Armors.Length);
        foreach(ArmorController _armor in Armors)
        {
            _armor.OnHit += DamageHandler;
            _armor.lightColor = RobotID < 100 ? 1 : 2;
        }
    }

    void OnDisable()
    {
        Debug.Log("Disable Armors: "+ Armors.Length);
        foreach(ArmorController _armor in Armors)
        {
            _armor.OnHit -= DamageHandler;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DamageHandler(int damageType, int armorID)
    {
        // GameManager.DamageHandlerServerRpc(damageType, armorID, RobotID);
    }

    [ClientRpc]
    public void UpdateRobotStatusClientRpc(float[,] status, ClientRpcParams clientRpcParams = default){
        if (IsOwner) return;
        
        Status = status;
    }
}
