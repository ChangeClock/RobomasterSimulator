using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    [Header("Game Status")]
    // 0 - Not started 1 - ready 2 - checking 3 - running 4 - ending
    [SerializeField] public NetworkVariable<int> GameStatus = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<float> timer = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private int TimePast = 0;

    // 0: 中立 1: R-Hero 2: R-Engineer 3/4/5: R-Infantry 6: R-Air 7: R-Sentry 9: R-Lidar 18: R-Outpost 19: R-Base 21: B-Hero 22: B-Engineer 23/24/25: B-Infantry 26: B-Air 27: B-Sentry 29: B-Lidar 38: B-Outpost 39: B-Base;
    public Dictionary<int, RefereeController> RefereeControllerList = new Dictionary<int, RefereeController>();

    private void OnEnable()
    {
        RefereeController.OnDamage += DamageUpload;
        RefereeController.OnShoot += ShootUpload;
        RefereeController.OnOccupy += OccupyUpload;
        RefereeController.OnSpawn += SpawnUpload;
    }

    private void OnDisable()
    {
        RefereeController.OnDamage -= DamageUpload;
        RefereeController.OnShoot -= ShootUpload;
        RefereeController.OnOccupy -= OccupyUpload;
        RefereeController.OnSpawn -= SpawnUpload;
    }

    private void Start() {
        // TODO: Need to register new referee controller dynamicallyRefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None); 
        
        // TODO: Initial the default RobotStatusList according to a config file;
        Debug.Log(JSONReader.LoadResourceTextfile("RMUC2023/RobotConfig.json"));

        RefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None); 
        Debug.Log(_list.Length);
        foreach(RefereeController _referee in _list)
        {
            RefereeControllerList.Add(_referee.RobotID, _referee);

            switch(_referee.RobotID){
                case 3:
                case 4:
                case 5:
                    RefereeControllerList[_referee.RobotID].HP.Value = (200);
                    break;
                case 18:
                case 38:
                    RefereeControllerList[_referee.RobotID].HP.Value = (1500);
                    break;
                case 19:
                case 39:
                    RefereeControllerList[_referee.RobotID].HP.Value = (5000);
                    break;
                default:
                    RefereeControllerList[_referee.RobotID].HP.Value = (500);
                    break;
            }

            Debug.Log("[GameController] _referee: " + _referee.gameObject.name + " " + _referee.RobotID);
            Debug.Log("[GameController] _referee: " + RefereeControllerList[_referee.RobotID].HP.Value);
        }
    }

    /**
    * These status need to be updated on fixed interval
    * 1. Game Info
    */

    void Update()
    {
        if (!IsServer) return;
        // TODO: Update Status Struct according RobotID to every RefreeController

        timer.Value += Time.deltaTime;
        TimePast = (int)timer.Value % 60;

        foreach(var _referee in RefereeControllerList.Values)
        {
            var networkObject = _referee.gameObject.GetComponent<NetworkObject>();
            if (!networkObject.IsSpawned) return;

            // ---------------Basic Status---------------//
            // Exp Sync
            // TODO: Calculate EXP during time

            // Revival & Immutable Status Sync
            if (_referee.Reviving.Value)
            {                    
                // TODO: Revived through purchase only have 3 seconds immutable, 100% HP, higher power and shooter will be enabled
                int _revivalTime = TimePast - _referee.TimePast.Value;
                
                if (_referee.Enabled.Value) {
                    
                    // Already revived, immutable for fixed amount of time
                    if (_referee.RevivalTime.Value + _revivalTime >= 10)
                    {
                        _referee.Reviving.Value = false;
                        _referee.Immutable.Value = false;
                    }else{
                        _referee.RevivalTime.Value += _revivalTime;
                    }

                } else {
                    
                    // Not revived yet, countdown revival time.
                    if (_revivalTime <= 0)
                    {
                        _referee.HP.Value = _referee.HPLimit.Value / 10;
                        _referee.RevivalTime.Value = 0;
                        _referee.Immutable.Value = true;
                        _referee.Enabled.Value = true;
                    } else {
                        // TODO: The progress will be 4 times quicked when detect reviving area.
                        _referee.RevivalTime.Value = _revivalTime;
                    }
                
                }
            }

            

            //--------------------Status only when enabled----------------------//
            if(!_referee.Enabled.Value) return;


            // Game Status Sync
            _referee.TimePast.Value = TimePast;

        }

        // Debug.Log("[GameController] HP: "+ RobotStatusList[18].GetHP());
    }

    /**
    * The following part is the handler for every events caught during the game
    * 0. Spawn Event -> Refree Controller
    * 1. Damage Event -> Armor Controller
    * 2. Shoot Event -> Shooter Controller
    * 3. Occupy Event -> RFID Controller
    * 4. Purchase Event -> Supply Controller
    * 5. Exchange Event -> Exchange Controller
    * 6. Warning Event -> Judge Controller
    * 7. Death Event -> Game Manager
    */

    void SpawnUpload(int robotID)
    {
        Debug.Log($"[GameManager] Referee {robotID} SpawnUpload");
        SpawnRefereeServerRpc(robotID);
    }

    [ServerRpc]
    void SpawnRefereeServerRpc(int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"[GameManager] Referee {robotID} ServerRPC");
        if (RefereeControllerList.ContainsKey(robotID))
        {
            Debug.LogError($"{robotID} exists !!!");
            return;
        }

        RefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None);
        Debug.Log($"[GameManager] List Length: {_list.Length}");
        foreach (RefereeController _referee in _list)
        {
            if (_referee.RobotID == robotID)
            {
                Debug.Log($"[GameManager] {robotID} referee added to gamemanager");
                RefereeControllerList.Add(_referee.RobotID, _referee);
                return;
            }
        }
    }

    void DamageUpload(int damageType, int armorID, int robotID)
    {
        DamageHandlerServerRpc(damageType, armorID, robotID);
    }

    [ServerRpc]
    void DamageHandlerServerRpc(int damageType, int armorID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("Damage Type: " + damageType + " Armor ID: " + armorID + " Robot ID: " + robotID);
        // Not Disabled or Immutable
        if (RefereeControllerList[robotID].Enabled.Value && !RefereeControllerList[robotID].Immutable.Value) {
            int _hp = RefereeControllerList[robotID].HP.Value;
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
            if (_hp - _damage <= 0)
            {
                RefereeControllerList[robotID].HP.Value = 0;
                RefereeControllerList[robotID].Enabled.Value = false;
                DeathHandlerServerRpc(robotID);
            } else {
                RefereeControllerList[robotID].HP.Value = (_hp - _damage);
            }
            
        }
    }

    void ShootUpload(int shooterID, int shooterType, int robotID, Vector3 userPosition, Vector3 shootVelocity)
    {
        ShootHandlerServerRpc(shooterID, shooterType, robotID);
    }

    [ServerRpc]
    void ShootHandlerServerRpc(int shooterID, int shooterType, int robotID, ServerRpcParams serverRpcParams = default)
    {
        switch(shooterID){
            case 0:
                RefereeControllerList[robotID].Heat0.Value += (shooterType == 0) ? 10 : 100;
                break;
            case 1:
                RefereeControllerList[robotID].Heat1.Value += (shooterType == 0) ? 10 : 100;
                break;
            default:
                Debug.LogWarning("[GameManager] Unknown shooter ID");
                break;
        }
    }

    void OccupyUpload(int areaID, int robotID)
    {
        OccupyHandlerServerRpc(areaID, robotID);
    }
    
    [ServerRpc]
    void OccupyHandlerServerRpc(int areaID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        // Debug.Log($"[GameManager] {robotID} occupied area {areaID}");

    }

    [ServerRpc]
    void DeathHandlerServerRpc(int robotID, ServerRpcParams serverRpcParams = default)
    {
        // TODO: Add EXP to Killer and Assistant
        switch (robotID)
        {
            case 7:
            case 27:
                // TODO: Change Base Status
                break;
            case 18:
            case 38:
                // TODO: Change Sentry & Base Status
                break;
            case 19:
            case 39:
                // TODO: Stop Game Handler
                break;
            default:
                // TODO: If the robot is penalty to death, disable revival
                // TODO: If the robot used purchase to revival, revival time will add 20s for each purchase
                RefereeControllerList[robotID].RevivalTime.Value = 10 + (420 - TimePast) / 10;
                RefereeControllerList[robotID].Reviving.Value = true;
                RefereeControllerList[robotID].Enabled.Value = false;
                break;
        }
    }
}
