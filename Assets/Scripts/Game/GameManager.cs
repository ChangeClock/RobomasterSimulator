using System;
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
    [SerializeField] public NetworkVariable<float> TimeLeft = new NetworkVariable<float>(0.0f);
    [SerializeField] private bool isRunning = false;

    [SerializeField] public List<RobotClass> KeyRobots = new List<RobotClass>()
    {
        RobotClass.Base, 
        RobotClass.Outpost, 
        RobotClass.Sentry
    };

    [SerializeField] public List<RobotClass> Buildings = new List<RobotClass>()
    {
        RobotClass.Base, 
        RobotClass.Outpost,
        RobotClass.Buff
    };

    [SerializeField] public RefereeController RedBase;
    [SerializeField] public RefereeController BlueBase;
    [SerializeField] public RefereeController RedOutpost;
    [SerializeField] public RefereeController BlueOutpost;
    [SerializeField] public RefereeController RedSentry;
    [SerializeField] public RefereeController BlueSentry;

    // 0: 中立 1: R-Hero 2: R-Engineer 3/4/5: R-Infantry 6: R-Air 7: R-Sentry 9: R-Lidar 18: R-Outpost 19: R-Base 21: B-Hero 22: B-Engineer 23/24/25: B-Infantry 26: B-Air 27: B-Sentry 29: B-Lidar 38: B-Outpost 39: B-Base;
    public Dictionary<int, RefereeController> RefereeControllerList = new Dictionary<int, RefereeController>();

    [Header("Buff")]
    [SerializeField] private BuffEffectSO ReviveBuff;
    [SerializeField] private BuffEffectSO PurchaseReviveBuff;

    [Header("Area")]
    [SerializeField] private AreaController[] RedPatrolPoints;
    [SerializeField] private AreaController[] BluePatrolPoints;

    // [Header("EXPInfo")]
    // [SerializeField] private ExpInfoSO HeroExpInfo;
    // [SerializeField] private ExpInfoSO InfantryExpInfo;

    private void OnEnable()
    {
        RefereeController.OnDamage += DamageUpload;
        RefereeController.OnShoot += ShootUpload;
        RefereeController.OnOccupy += OccupyUpload;
        RefereeController.OnSpawn += SpawnUpload;
        RefereeController.OnRevived += ReviveUpload;
        RefereeController.OnDeath += DeathUpload;
    }

    private void OnDisable()
    {
        RefereeController.OnDamage -= DamageUpload;
        RefereeController.OnShoot -= ShootUpload;
        RefereeController.OnOccupy -= OccupyUpload;
        RefereeController.OnSpawn -= SpawnUpload;
        RefereeController.OnRevived -= ReviveUpload;
        RefereeController.OnDeath -= DeathUpload;
    }

    void Start() 
    {
        if (!IsServer) return;
    }

    /**
    * These status need to be updated on fixed interval
    * 1. Game Info
    */

    void FixedUpdate()
    {
        if (!IsServer) return;

        // Debug.Log(RefereeControllerList.Count);

        String IDList = "";

        foreach(var _referee in RefereeControllerList.Values)
        {
            // ---------------Basic Status---------------//
            
            IDList += _referee.RobotID.Value + " ";

            // if (_referee.RobotID.Value == 2) Debug.Log($"{_referee.RobotID.Value} Robot Shooter0 Enabled? {_referee.Shooter0Enabled.Value}");

            //--------------------Status only when robot is enabled ----------------------//
            if (_referee.Enabled.Value)
            {

            //--------------------Status only when game is running ----------------------//
                if (isRunning) 
                {
                    // Time Update
                    TimeLeft.Value -= Time.deltaTime;

                    if (TimeLeft.Value <= 0.0f) FinishGame();
                }
            }

        }

        // Debug.Log("[GameManager] IDList: " + IDList);

        // Debug.Log("[GameController] HP: "+ RobotStatusList[18].GetHP());
    }

    public void FinishGame()
    {
        isRunning = false;

        // TODO: Reset robot status, play ending
    }

    public void StartGame()
    {
        isRunning = true;

        // TODO: bring robot to their spawnpoint, reset HP, EXP, Level, Ammo, Heat, Energy

        // TODO: Reset bank on each side, reset mine ore status.
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
            if (_referee.RobotID.Value == robotID)
            {
                Debug.Log($"[GameManager] {robotID} referee added to gamemanager");
                RefereeControllerList.Add(_referee.RobotID.Value, _referee);
                return;
            }
        }
    }

    void RefereeDamage(float damage, int robotID)
    {
        if (RefereeControllerList[robotID].Enabled.Value)
        {
            float _hp = RefereeControllerList[robotID].HP.Value;
            
            // Debug.Log($"[GameManager] raw damage: {damage}, damage {damage}");

            if (_hp - damage <= 0)
            {
                RefereeControllerList[robotID].HP.Value = 0;
                RefereeControllerList[robotID].Enabled.Value = false;
                DeathHandlerServerRpc(robotID);
            } else {
                RefereeControllerList[robotID].HP.Value = (_hp - damage);
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
            float _hp = RefereeControllerList[robotID].HP.Value;
            float _damage = 0f;
            // Debug.Log("[GameManager - Damage] HP:"+RobotStatusList[robotID].HP);
            switch(damageType){
                case 0:
                    _damage = 2f;
                    break;
                case 1:
                    _damage = 10f;
                    break;
                case 2:
                    _damage = 100f;
                    break;
                case 3:
                    _damage = 750f;
                    break;
                default:
                    Debug.LogWarning("Unknown Damage Type" + damageType);
                    break;
            }

            if (RefereeControllerList[robotID].DEFBuff.Value > 0) _damage = _damage * RefereeControllerList[robotID].DEFBuff.Value / 100;

            // Add ATKBuff accoding to attacker ID;

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

    void DeathUpload(int id)
    {
        DeathHandlerServerRpc(id);
    }

    [ServerRpc]
    void DeathHandlerServerRpc(int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"Robot {robotID} death event!");

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
                RefereeControllerList[robotID].MaxReviveProgress.Value = (int)Math.Round(10.0f + TimeLeft.Value / 10.0f);
                RefereeControllerList[robotID].Reviving.Value = true;
                RefereeControllerList[robotID].Enabled.Value = false;
                break;
        }
    }

    void ReviveUpload(int id, int mode)
    {
        ReviveHandlerServerRpc(id, mode);
    }

    [ServerRpc]
    void ReviveHandlerServerRpc(int id, int mode, ServerRpcParams serverRpcParams = default)
    {
        switch(mode)
        {
            case 0:
                RefereeControllerList[id].AddBuff(ReviveBuff);
                break;
            case 1:
                RefereeControllerList[id].AddBuff(PurchaseReviveBuff);
                break;
            default:
                Debug.Log("[GameManager] Unknown revive mode");
                break;
        }
    }
}
