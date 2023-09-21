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
    }

    private void OnDisable()
    {
        RefereeController.OnDamage -= DamageUpload;
        RefereeController.OnShoot -= ShootUpload;
        RefereeController.OnOccupy -= OccupyUpload;
        RefereeController.OnSpawn -= SpawnUpload;
        RefereeController.OnRevived -= ReviveUpload;
    }

    private void Start() {
        // TODO: Need to register new referee controller dynamicallyRefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None); 
        
        // TODO: Initial the default RobotStatusList according to a config file;
        // Debug.Log(JSONReader.LoadResourceTextfile("RMUC2023/RobotConfig.json"));

        // RefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None); 
        // Debug.Log(_list.Length);
        // foreach(RefereeController _referee in _list)
        // {
        //     RefereeControllerList.Add(_referee.RobotID.Value, _referee);

        //     switch(_referee.RobotID.Value){
        //         case 3:
        //         case 4:
        //         case 5:
        //             RefereeControllerList[_referee.RobotID.Value].HP.Value = (200);
        //             break;
        //         case 18:
        //         case 38:
        //             RefereeControllerList[_referee.RobotID.Value].HP.Value = (1500);
        //             break;
        //         case 19:
        //         case 39:
        //             RefereeControllerList[_referee.RobotID.Value].HP.Value = (5000);
        //             break;
        //         default:
        //             RefereeControllerList[_referee.RobotID.Value].HP.Value = (500);
        //             break;
        //     }

        //     Debug.Log("[GameController] _referee: " + _referee.gameObject.name + " " + _referee.RobotID.Value);
        //     Debug.Log("[GameController] _referee: " + RefereeControllerList[_referee.RobotID.Value].HP.Value);
        // }

        // Initial NPCS already in the game scene

        if (!IsServer) return;

        // RefereeController.OnDamage += DamageUpload;
        
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
            // var networkObject = _referee.gameObject.GetComponent<NetworkObject>();
            // if (!networkObject.IsSpawned) return;

            // ---------------Basic Status---------------//
            
            IDList += _referee.RobotID.Value + " ";

            // if (_referee.RobotID.Value == 2) Debug.Log($"{_referee.RobotID.Value} Robot Shooter0 Enabled? {_referee.Shooter0Enabled.Value}");

            // Heat Status

            int CDBuff = 1;
            if (_referee.CDBuff.Value > 0) CDBuff = _referee.CDBuff.Value;

            if (_referee.Shooter0Enabled.Value & _referee.Heat0.Value > 0)
            {
                // Shooter 0 overheating
                if (_referee.Heat0.Value > _referee.Heat0Limit.Value & _referee.Heat0.Value <= 2*_referee.Heat0Limit.Value)
                {
                    RefereeDamage((_referee.Heat0.Value - _referee.Heat0Limit.Value) / 250 / 10 * _referee.HPLimit.Value , _referee.RobotID.Value);
                } else if (_referee.Heat0.Value >= 2*_referee.Heat0Limit.Value) {
                    RefereeDamage((_referee.Heat0.Value - 2*_referee.Heat0Limit.Value) / 250 * _referee.HPLimit.Value, _referee.RobotID.Value);
                    _referee.Heat0.Value = 2*_referee.Heat0Limit.Value;
                }
                
                // Shooter 0 CD
                if (_referee.Heat0.Value >= _referee.CD0.Value * CDBuff * Time.deltaTime) 
                {
                    _referee.Heat0.Value -= _referee.CD0.Value * CDBuff * Time.deltaTime;
                } else {
                    _referee.Heat0.Value = 0;
                }
            }

            if (_referee.Shooter1Enabled.Value & _referee.Heat1.Value > 0)
            {
                // Shooter 1 overheating
                if (_referee.Heat1.Value > _referee.Heat1Limit.Value & _referee.Heat1.Value <= 2*_referee.Heat1Limit.Value)
                {
                    RefereeDamage((_referee.Heat1.Value - _referee.Heat1Limit.Value) / 250 / 10 * _referee.HPLimit.Value , _referee.RobotID.Value);
                } else if (_referee.Heat1.Value >= 2*_referee.Heat1Limit.Value) {
                    RefereeDamage((_referee.Heat1.Value - 2*_referee.Heat1Limit.Value) / 250 * _referee.HPLimit.Value, _referee.RobotID.Value);
                    _referee.Heat1.Value = 2*_referee.Heat1Limit.Value;
                }

                if (_referee.Heat1.Value >= _referee.CD1.Value * CDBuff * Time.deltaTime) 
                {
                    _referee.Heat1.Value -= _referee.CD1.Value * CDBuff * Time.deltaTime;
                } else {
                    _referee.Heat1.Value = 0;
                }
            }

            // Revival & Immutable Status Sync
            // if (_referee.Reviving.Value && !KeyRobots.Contains(_referee.robotClass.Value))
            // {                    
            //     // TODO: Revived through purchase only have 3 seconds immutable, 100% HP, higher power and shooter will be enabled
            //     _referee.CurrentReviveProgress.Value += _referee.ReviveProgressPerSec.Value * Time.deltaTime;

            //     if (_referee.CurrentReviveProgress.Value >= _referee.MaxReviveProgress.Value)
            //     {
            //         Debug.Log($"Robot {_referee.RobotID.Value} revived!");
            //         _referee.Reviving.Value = false;
            //         _referee.CurrentReviveProgress.Value = 0;
            //         _referee.HP.Value = _referee.HPLimit.Value * 10 / 100;
            //         _referee.Enabled.Value = true;
            //         _referee.AddBuff(ReviveBuff);
            //     }
            // }

            //--------------------Status only when robot is enabled ----------------------//
            if (_referee.Enabled.Value)
            {
                // Buff Sync
                // if (!Buildings.Contains(_referee.robotClass.Value))
                // {
                //     BuffEffectSO newBuffStat = Instantiate(_referee.defaultBuff);

                //     if (_referee.activeBuffs.Count > 0)
                //     {
                //         var _activeBuffsCache = _referee.activeBuffs.Values;
                //         List<BuffEffectSO> overtimeBuff = new List<BuffEffectSO>();

                //         foreach (var _buffInfo in _activeBuffsCache)
                //         {
                //             var _buff = _buffInfo.buffEffect;

                //             if (_buff.DEFBuff > newBuffStat.DEFBuff)
                //             {
                //                 newBuffStat.DEFBuff = _buff.DEFBuff;
                //             }

                //             if (_buff.ATKBuff > newBuffStat.ATKBuff)
                //             {
                //                 newBuffStat.ATKBuff = _buff.ATKBuff;
                //             }

                //             if (_buff.CDBuff > newBuffStat.CDBuff)
                //             {
                //                 newBuffStat.CDBuff = _buff.CDBuff;
                //             }

                //             if (_buff.ReviveProgressPerSec > newBuffStat.ReviveProgressPerSec)
                //             {
                //                 newBuffStat.ReviveProgressPerSec = _buff.ReviveProgressPerSec;
                //             }

                //             if (_buff.HealBuff > newBuffStat.HealBuff)
                //             {
                //                 newBuffStat.HealBuff = _buff.HealBuff;
                //             }

                //             _buffInfo.lastTime += Time.deltaTime;
                //             if (_buffInfo.lastTime > _buff.buffDuration) overtimeBuff.Add(_buff);
                //         }

                //         if (overtimeBuff.Count > 0)
                //         {
                //             foreach(var _buff in overtimeBuff)
                //             {
                //                 _referee.RemoveBuff(_buff);
                //             }
                //         }
                //     }

                //     _referee.HealBuff.Value = newBuffStat.HealBuff;
                //     _referee.DEFBuff.Value = newBuffStat.DEFBuff;
                //     _referee.ATKBuff.Value = newBuffStat.ATKBuff;
                //     _referee.CDBuff.Value = newBuffStat.CDBuff;
                //     _referee.ReviveProgressPerSec.Value = newBuffStat.ReviveProgressPerSec;
                // }

                // if (_referee.HP.Value < _referee.HPLimit.Value && _referee.HealBuff.Value > 0 && !Buildings.Contains(_referee.robotClass.Value))
                // {
                //     float _recoverHP = _referee.HPLimit.Value * _referee.HealBuff.Value / 100 * Time.deltaTime;

                //     if (_referee.HP.Value + _recoverHP < _referee.HPLimit.Value)
                //     {
                //         _referee.HP.Value += _recoverHP;
                //     } else {
                //         _referee.HP.Value = _referee.HPLimit.Value;
                //     }
                // }

            //--------------------Status only when game is running ----------------------//
                if (isRunning) 
                {
                    // Time Update
                    TimeLeft.Value -= Time.deltaTime;

                    if (TimeLeft.Value <= 0.0f) FinishGame();

                    // Exp & Level Up Sync
                    // Full level robots & non-hero/infantry robot will skip

                    // if (_referee.Level.Value < 3 && (_referee.robotClass.Value == RobotClass.Hero || _referee.robotClass.Value == RobotClass.Infantry))
                    // {
                    //     _referee.TimeToNextEXP.Value += Time.deltaTime;
                        
                    //     switch (_referee.robotClass.Value)
                    //     {
                    //         case RobotClass.Hero:
                    //             // EXP growth with time
                    //             if (_referee.TimeToNextEXP.Value >= HeroExpInfo.expGrowth)
                    //             {
                    //                 _referee.TimeToNextEXP.Value -= HeroExpInfo.expGrowth;
                    //                 _referee.EXP.Value += 1;
                    //             }

                    //             // Level up
                    //             if (_referee.EXP.Value >= HeroExpInfo.expToNextLevel[_referee.Level.Value])
                    //             {
                    //                 // Don't zero the current EXP, just minus the EXP needed to next level
                    //                 _referee.EXP.Value -= HeroExpInfo.expToNextLevel[_referee.Level.Value];
                    //                 _referee.Level.Value += 1;

                    //                 // TODO: Update performance

                    //             }
                    //             break;
                    //         case RobotClass.Infantry:
                    //             // EXP growth with time
                    //             if (_referee.TimeToNextEXP.Value >= InfantryExpInfo.expGrowth)
                    //             {
                    //                 _referee.TimeToNextEXP.Value -= InfantryExpInfo.expGrowth;
                    //                 _referee.EXP.Value += 1;
                    //             }

                    //             // Level up
                    //             if (_referee.EXP.Value >= InfantryExpInfo.expToNextLevel[_referee.Level.Value])
                    //             {
                    //                 // Don't zero the current EXP, just minus the EXP needed to next level
                    //                 _referee.EXP.Value -= InfantryExpInfo.expToNextLevel[_referee.Level.Value];
                    //                 _referee.Level.Value += 1;

                    //                 // TODO: Update performance

                    //             }
                    //             break;
                    //         default:
                    //             break;
                    //     }

                    // } else if (_referee.Level.Value == 0) {
                    //     // Haven't choose performance, just addon the EXP.
                    //     _referee.TimeToNextEXP.Value += Time.deltaTime;
                    // }
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
