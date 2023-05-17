using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // public struct RobotStatus
    // {
    //     // 0: 中立 1: R-Hero 2: R-Engineer 3/4/5: R-Infantry 6: R-Air 7: R-Sentry 9: R-Lidar 98: R-Outpost 99: R-Base 101: B-Hero 102: B-Engineer 103/104/105: B-Infantry 106: B-Air 107: B-Sentry 109: B-Lidar 198: B-Outpost 199: B-Base;
    //     public int RobotID;
    //     public int HPLimit;
    //     public int HP;
    //     public int[] ShooterEnable;
    //     public int[] HeatLimit;
    //     // public int[] Heat;
    //     public int[] CD;
    //     public int[] SpeedLimit;
    //     // public int[] Speed = {0,0};
    //     public int PowerLimit;
    //     // public int Power = 0;
    //     public int Level;
    //     public bool Disabled;
    //     public bool Immutable;

    //     public RobotStatus(int robotID){
    //         RobotID = robotID;

    //         HPLimit = 1500;
    //         HP = 1500;
    //         ShooterEnable = new int[] {0,0};
    //         HeatLimit = new int[] {0,0};
    //         CD = new int[] {0,0};
    //         SpeedLimit = new int[] {0,0};
    //         PowerLimit = -1;
    //         Level = 0;
    //         Disabled = true;
    //         Immutable = true;
    //     }
    // }

    private float[,] RobotStatusList = new float[15,16];

    private GameObject[] NPC = new GameObject[2];

    void Start()
    {
        NPC[0] = transform.Find("R-Outpost").gameObject;
        NPC[1] = transform.Find("B-Outpost").gameObject;
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    void Update()
    {
        foreach(GameObject _NPC in NPC){
            if (_NPC != null) _NPC.GetComponent<RefereeController>().UpdateRobotStatusClientRpc(RobotStatusList);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void DamageHandlerServerRpc(int damageType, int armorID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("Damage Type: " + damageType + " Armor ID: " + armorID + " Robot ID: " + robotID);
    }
}
