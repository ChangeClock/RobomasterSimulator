using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // 0: 中立 1: R-Hero 2: R-Engineer 3/4/5: R-Infantry 6: R-Air 7: R-Sentry 9: R-Lidar 18: R-Outpost 19: R-Base 21: B-Hero 22: B-Engineer 23/24/25: B-Infantry 26: B-Air 27: B-Sentry 29: B-Lidar 38: B-Outpost 39: B-Base;
    public Dictionary<int, RefereeController> RefereeControllerList = new Dictionary<int, RefereeController>();

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        // TODO: Unsubscribe damage event of every referee controller
        var enumerator = RefereeControllerList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            enumerator.Current.Value.OnDamage -= DamageUpload;
        }
    }

    private void Start() {
        // TODO: Need to register new referee controller dynamicallyRefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None); 
        
        RefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None); 
        Debug.Log(_list.Length);
        foreach(RefereeController _refree in _list)
        {
            RefereeControllerList.Add(_refree.RobotID, _refree);
            _refree.OnDamage += DamageUpload;
            // Initial the default RobotStatusList according to a config file;

            switch(_refree.RobotID){
                case 18:
                case 38:
                    RefereeControllerList[_refree.RobotID].SetHP(1500);
                    break;
                case 19:
                case 39:
                    RefereeControllerList[_refree.RobotID].SetHP(5000);
                    break;
                default:
                    RefereeControllerList[_refree.RobotID].SetHP(500);
                    break;
            }

            Debug.Log("[GameController] _refree: " + _refree.gameObject.name + " " + _refree.RobotID);
            Debug.Log("[GameController] _refree: " + RefereeControllerList[_refree.RobotID].GetHP());
        }
    }

    void Update()
    {
        // TODO: Update Status Struct according RobotID to every RefreeController
        var enumerator = RefereeControllerList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            int _id = enumerator.Current.Key;
            
            // update something...
        }

        // Debug.Log("[GameController] HP: "+ RobotStatusList[18].GetHP());

        // TODO: Need to subscribe and unsubscribe the damage event when a robot / referee controller was removed from the game.
    }

    void DamageUpload(int damageType, int armorID, int robotID)
    {
        DamageHandlerServerRpc(damageType, armorID, robotID);
    }

    [ServerRpc]
    public void DamageHandlerServerRpc(int damageType, int armorID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("Damage Type: " + damageType + " Armor ID: " + armorID + " Robot ID: " + robotID);
        // Not Disabled or Immutable
        if (!RefereeControllerList[robotID].GetDisabled() && !RefereeControllerList[robotID].GetImmutable()) {
            int _hp = RefereeControllerList[robotID].GetHP();
            int _damage = 0;
            // Debug.Log("[GameManager - Damage] HP:"+RobotStatusList[robotID].HP);
            switch(damageType){
                case 0:
                    _damage = 2;
                    break;
                case 1:
                    _damage = 10;
                    break;
                case 2:
                    _damage = 100;
                    break;
                case 3:
                    _damage = 750;
                    break;
                default:
                    Debug.LogWarning("Unknown Damage Type" + damageType);
                    break;
            }
            RefereeControllerList[robotID].SetHP(_hp - _damage);
        }
    }
}
