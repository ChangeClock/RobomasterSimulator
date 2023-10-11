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
    [SerializeField] public NetworkVariable<int> GameStatus = new NetworkVariable<int>(0);
    [SerializeField] public NetworkVariable<float> TimeLeft = new NetworkVariable<float>(420.0f);
    [SerializeField] public NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false);

    [SerializeField] private List<Faction> Factions = new List<Faction>(){Faction.Red, Faction.Blue};

    [SerializeField] public RefereeController RedBase;
    [SerializeField] public RefereeController BlueBase;
    [SerializeField] public RefereeController RedOutpost;
    [SerializeField] public RefereeController BlueOutpost;
    [SerializeField] public RefereeController RedSentry;
    [SerializeField] public RefereeController BlueSentry;

    // 0: 中立 1: R-Hero 2: R-Engineer 3/4/5: R-Infantry 6: R-Air 7: R-Sentry 9: R-Lidar 18: R-Outpost 19: R-Base 21: B-Hero 22: B-Engineer 23/24/25: B-Infantry 26: B-Air 27: B-Sentry 29: B-Lidar 38: B-Outpost 39: B-Base;
    public Dictionary<int, RefereeController> RefereeControllerList = new Dictionary<int, RefereeController>();

    [Header("Tags")]
    [SerializeField] private List<RobotClass> GroundUnit = new List<RobotClass>(){RobotClass.Hero, RobotClass.Engineer, RobotClass.Infantry, RobotClass.Sentry};
    [SerializeField] private List<RobotClass> GrowingUnit = new List<RobotClass>(){RobotClass.Hero, RobotClass.Infantry};
    
    [Header("EXP")]
    public NetworkVariable<bool> HasFirstBlood = new NetworkVariable<bool>(false);
    [SerializeField] private ExpInfoSO HeroExpInfo;
    [SerializeField] private ExpInfoSO InfantryExpInfo;
    [SerializeField] private ExpInfoSO EngineerExpInfo;
    [SerializeField] private ExpInfoSO SentryExpInfo;

    [Header("Performance")]
    [SerializeField] private RobotPerformanceSO[] HeroChassisPerformance;
    [SerializeField] private RobotPerformanceSO[] GimbalPerformance_17mm;
    [SerializeField] private RobotPerformanceSO[] InfantryChassisPerformance;
    [SerializeField] private RobotPerformanceSO[] GimbalPerformance_42mm;
    [SerializeField] private RobotPerformanceSO EngineerPerformance;
    [SerializeField] private RobotPerformanceSO SentryPerformance;

    [Header("Buff")]
    [SerializeField] private BuffEffectSO DefaultBuff;
    [SerializeField] private BuffEffectSO SentryDefaultBuff;
    [SerializeField] private BuffEffectSO ReviveBuff;
    [SerializeField] private BuffEffectSO PurchaseReviveBuff;

    [Header("Area")]
    [SerializeField] private AreaController[] RedPatrolPoints;
    [SerializeField] private AreaController[] BluePatrolPoints;
    [SerializeField] private AreaController RedRepairStation;
    [SerializeField] private AreaController BlueRepairStation;

    // [Header("Coin")]
    public int InitialCoin = 400;

    [Header("Purchase")]    
    public NetworkVariable<int[]> Coins = new NetworkVariable<int[]>();
    public NetworkVariable<int[]> CoinsTotal = new NetworkVariable<int[]>();
    public NetworkVariable<bool> HasFirstGold = new NetworkVariable<bool>(false);
    [SerializeField] private int RemoteSupplyApplyInterval = 6;
    [SerializeField] private int RemoteHPTimesLimit = 2;
    [SerializeField] private float RemoteHPSupllyCount = 0.6f;
    public NetworkVariable<int[]> RemoteHPTimes = new NetworkVariable<int[]>();
    [SerializeField] private int Remote17mmTimesLimit = 2;
    [SerializeField] private int Remote17mmSupplyCount = 100;
    public NetworkVariable<int[]> Remote17mmTimes = new NetworkVariable<int[]>();
    [SerializeField] private int Remote42mmTimesLimit = 2;
    [SerializeField] private int Remote42mmSupplyCount = 10;
    public NetworkVariable<int[]> Remote42mmTimes = new NetworkVariable<int[]>();


    // [Header("EXPInfo")]
    // [SerializeField] private ExpInfoSO HeroExpInfo;
    // [SerializeField] private ExpInfoSO InfantryExpInfo;

    protected virtual void OnEnable()
    {
        RefereeController.OnDamage += DamageUpload;
        RefereeController.OnShoot += ShootUpload;
        RefereeController.OnOccupy += OccupyUpload;
        RefereeController.OnSpawn += SpawnUpload;
        // RefereeController.OnLevelup += LevelupUpload;
        RefereeController.OnPerformanceChange += ChangePerformanceUpload;
        RefereeController.OnRevived += ReviveUpload;
        RefereeController.OnDeath += DeathUpload;
        RefereeController.OnReady += ReadyUpload;
        RefereeController.OnPurchase += PurchaseUpload;
    }

    protected virtual void OnDisable()
    {
        RefereeController.OnDamage -= DamageUpload;
        RefereeController.OnShoot -= ShootUpload;
        RefereeController.OnOccupy -= OccupyUpload;
        RefereeController.OnSpawn -= SpawnUpload;
        // RefereeController.OnLevelup -= LevelupUpload;
        RefereeController.OnPerformanceChange -= ChangePerformanceUpload;
        RefereeController.OnRevived -= ReviveUpload;
        RefereeController.OnDeath -= DeathUpload;
        RefereeController.OnReady -= ReadyUpload;
        RefereeController.OnPurchase -= PurchaseUpload;
    }

    void Start() 
    {
        if (!IsServer) return;

        ResetCoin();

        ResetRemoteSupplyTimes();
    }

    protected virtual void Update()
    {
        if (!IsServer) return;
    }

    protected virtual void FixedUpdate()
    {
        if (!IsServer) return;

        if (isRunning.Value) 
        {
            OnTimeLeftChange(TimeLeft.Value, TimeLeft.Value - Time.deltaTime);
            // Time Update
            TimeLeft.Value -= Time.deltaTime;

            if (TimeLeft.Value <= 0.0f) FinishGame();
        }
    }

    protected virtual void OnTimeLeftChange(float oldTime, float newTime)
    {

    }

    public void BaseShieldOff(Faction faction)
    {
        switch (faction)
        {
            case Faction.Red:
                RedBase.Shield.Value = 0;
                RedBase.ShieldOff();
                break;
            case Faction.Blue:
                BlueBase.Shield.Value = 0;
                BlueBase.ShieldOff();
                break;
            default:
                break;
        }
    }

    public void FinishGame(Faction faction = Faction.Neu)
    {
        isRunning.Value = false;

        // RedBase.Reset();
        // BlueBase.Reset();
        // RedOutpost.Reset();
        // BlueOutpost.Reset();

        foreach(var _referee in RefereeControllerList.Values)
        {
            _referee.Reset();
        }

        // TODO: Reset robot status, play ending
    }

    public virtual void StartGame()
    {
        HasFirstBlood.Value = false;
        HasFirstGold.Value = false;

        ResetCoin();
        ResetRemoteSupplyTimes();

        foreach(var _referee in RefereeControllerList.Values)
        {
            _referee.Reset();
        }

        TimeLeft.Value = 420.0f;

        isRunning.Value = true;

        // TODO: bring robot to their spawnpoint, reset HP, EXP, Level, Ammo, Heat, Energy

        // TODO: Reset bank on each side, reset mine ore status.
    }

    protected void AddCoin(Faction faction, int coin)
    {
        Coins.Value[(int)faction] += coin;
        CoinsTotal.Value[(int)faction] += coin;
    }

    protected void ResetCoin()
    {
        foreach (var fac in Factions)
        {
            Coins.Value[(int)fac] = InitialCoin;
            CoinsTotal.Value[(int)fac] = InitialCoin;
        }
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
                
                if (_referee.robotClass.Value == RobotClass.Sentry)
                {
                    _referee.defaultBuff = SentryDefaultBuff;
                } else if (!_referee.robotTags.Contains(RobotTag.Building))
                {
                    _referee.defaultBuff = DefaultBuff;
                }

                return;
            }
        }
    }

    void ChangePerformanceUpload(int robotID, int chassisMode, int shooter1Mode, int shooter2Mode)
    {
        ChangePerformanceServerRpc(robotID, chassisMode, shooter1Mode, shooter2Mode);
    }

    [ServerRpc]
    void ChangePerformanceServerRpc(int robotID, int chassisMode, int shooter1Mode, int shooter2Mode, ServerRpcParams serverRpcParams = default)
    {
        RefereeController referee = RefereeControllerList[robotID];

        if (isRunning.Value) return;

        switch (referee.robotClass.Value)
        {
            case RobotClass.Hero:
                SetUnitPerformance(referee, chassisMode, HeroChassisPerformance[chassisMode]);
                SetUnitEXPInfo(referee, HeroExpInfo);
                break;
            case RobotClass.Infantry:
                SetUnitPerformance(referee, chassisMode, InfantryChassisPerformance[chassisMode]);
                SetUnitEXPInfo(referee, InfantryExpInfo);
                break;
            case RobotClass.Engineer:
                SetUnitPerformance(referee, chassisMode, EngineerPerformance);
                SetUnitEXPInfo(referee, EngineerExpInfo);
                break;
            case RobotClass.Sentry:
                SetUnitPerformance(referee, chassisMode, SentryPerformance);
                SetUnitEXPInfo(referee, SentryExpInfo);
                break;
            default:
                break;
        }
        
        int _mode;

        foreach (var _shooter in referee.ShooterControllerList.Values)
        {
            if (!_shooter.Enabled.Value) continue;

            _mode = 0;
            if (_shooter.ID == 0) _mode = shooter1Mode;
            if (_shooter.ID == 1) _mode = shooter2Mode;

            switch (_shooter.Type.Value)
            {
                case 0:
                    SetShooterPerformance(_shooter, _mode, GimbalPerformance_17mm[_mode]);
                    break;
                case 1:
                    SetShooterPerformance(_shooter, _mode, GimbalPerformance_42mm[_mode]);
                    break;
                default:
                    break;
            }
        }
    }

    void SetUnitEXPInfo(RefereeController referee, ExpInfoSO expInfo)
    {
        referee.EXPInfo = expInfo;
        if (expInfo.expToNextLevel.Length > referee.Level.Value)
        {       
            referee.EXPToNextLevel.Value = expInfo.expToNextLevel[referee.Level.Value];
        }
        if (expInfo.expValue.Length > referee.Level.Value)
        {
            referee.EXPValue.Value = expInfo.expValue[referee.Level.Value];
        }
    }

    void SetUnitPerformance(RefereeController referee, int mode, RobotPerformanceSO performance)
    {
        Debug.Log($"[GameManager] mode: {mode}, maxHealth: {performance.maxHealth[referee.Level.Value]}, level: {referee.Level.Value}");
        referee.ChassisMode.Value = mode;
        referee.ChassisPerformance = performance;
        float _recoverHP = performance.maxHealth[referee.Level.Value] - referee.HPLimit.Value;
        referee.HPLimit.Value = performance.maxHealth[referee.Level.Value];
        referee.HP.Value += _recoverHP;
        referee.PowerLimit.Value = performance.maxPower[referee.Level.Value];
    }

    void SetShooterPerformance(ShooterController shooter, int mode, RobotPerformanceSO performance)
    {
        shooter.Mode.Value = mode;
        shooter.GimbalPerformance = performance;
        shooter.HeatLimit.Value = performance.maxHeat[shooter.Level.Value];
        shooter.CD.Value = performance.coolDown[shooter.Level.Value];
        shooter.SpeedLimit.Value = performance.shootSpeed[shooter.Level.Value];
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
                DeathHandlerServerRpc(0, robotID);
            } else {
                RefereeControllerList[robotID].HP.Value = (_hp - damage);
            }
        }
    }

    void DamageUpload(int damageType, float damage, int armorID, int attackerID, int robotID)
    {
        DamageHandlerServerRpc(damageType, damage, armorID, attackerID, robotID);
    }

    [ServerRpc]
    void DamageHandlerServerRpc(int damageType, float damage, int armorID, int attackerID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("Damage Type: " + damageType + " Armor ID: " + armorID + " Robot ID: " + robotID);
        // Not Disabled or Immutable

        switch(damageType){
            case 0:
            case 1:
            case 2:
                break;
            case 3:
                // Missle damage
                // TODO: Add blind to robotID belong team
                break;
            default:
                Debug.LogWarning("Unknown Damage Type" + damageType);
                break;
        }
    }

    void ShootUpload(int shooterID, int shooterType, int robotID, Vector3 userPosition, Vector3 shootVelocity)
    {
        ShootHandlerServerRpc(shooterID, shooterType, robotID);
    }

    [ServerRpc]
    protected virtual void ShootHandlerServerRpc(int shooterID, int shooterType, int robotID, ServerRpcParams serverRpcParams = default)
    {
        
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

    void DeathUpload(int attackerID, int id)
    {
        DeathHandlerServerRpc(attackerID, id);
    }

    [ServerRpc]
    void DeathHandlerServerRpc(int attackerID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"Robot {robotID} death event!");

        RefereeController Victim = RefereeControllerList[robotID];

        // TODO: Add EXP to Killer and Assistant
        switch (robotID)
        {
            // TODO: Change Base Status
            case 7:
                if (!RedOutpost.Enabled.Value) BaseShieldOff(Faction.Red);
                break;
            case 27:
                if (!BlueOutpost.Enabled.Value) BaseShieldOff(Faction.Blue);
                break;

            // TODO: Change Sentry & Base Status
            case 18:
                if (RedSentry == null || !RedSentry.Enabled.Value) 
                {
                    BaseShieldOff(Faction.Red);
                } else {
                    RedSentry.Immutable.Value = false;
                    RedBase.Immutable.Value = false;
                }
                break;
            case 38:
                if (BlueSentry == null || !BlueSentry.Enabled.Value)
                {
                    BaseShieldOff(Faction.Blue);
                } else {
                    BlueSentry.Immutable.Value = false;
                    BlueBase.Immutable.Value = false;
                }
                break;
            
            // TODO: Stop Game Handler
            case 19:
                FinishGame(Faction.Blue);
                break;
            case 39:
                FinishGame(Faction.Red);
                break;
            default:
                // TODO: If the robot is penalty to death, disable revival
                Victim.MaxReviveProgress.Value = (int)Math.Round(10.0f + TimeLeft.Value / 10.0f);
                Victim.Reviving.Value = true;
                Victim.Enabled.Value = false;
                break;
        }

        if (!Victim.robotTags.Contains(RobotTag.GroundUnit)) return;

        if (Victim.AttackList.Count > 0)
        {
            foreach (var _id in Victim.AttackList.Keys)
            {
                if (RefereeControllerList[_id].faction.Value != Victim.faction.Value) 
                {
                    RefereeControllerList[_id].EXP.Value += (_id == attackerID) ? Victim.EXP.Value + (HasFirstBlood.Value ? 50 : 0) : Victim.EXP.Value / 4;
                }
            }
        } else {
            int _expInTotal = Victim.EXPValue.Value + (HasFirstBlood.Value ? 50 : 0);
            
            DistributeEXP(Victim.faction.Value == Faction.Red ? Faction.Blue : Faction.Red, _expInTotal);
        }

        if (!HasFirstBlood.Value)
        {
            HasFirstBlood.Value = true;
        }
    }

    protected void DistributeEXP(Faction faction, int exp)
    {
        List<int> _idList = new List<int>();

        foreach (var _referee in RefereeControllerList.Values)
        {
            if (_referee.faction.Value == faction
            && (_referee.robotClass.Value == RobotClass.Infantry || _referee.robotClass.Value == RobotClass.Hero) 
            && _referee.Enabled.Value
            ) _idList.Add(_referee.RobotID.Value); 
        }

        foreach (var _id in _idList)
        {
            RefereeControllerList[_id].EXP.Value += exp / _idList.Count;
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
                // TODO: If the robot used purchase to revival, revival time will add 20s for each purchase
                RefereeControllerList[id].MaxReviveProgress.Value += 20;
                RefereeControllerList[id].AddBuff(PurchaseReviveBuff);
                break;
            default:
                Debug.Log("[GameManager] Unknown revive mode");
                break;
        }
    }

    void ReadyUpload(int id)
    {
        ReadyHandlerServerRpc(id);
    }

    [ServerRpc]
    void ReadyHandlerServerRpc(int id, ServerRpcParams serverRpcParams = default)
    {
        RefereeController referee = RefereeControllerList[id];

        if (isRunning.Value) return;

        bool allReady = true;

        foreach (var _referee in RefereeControllerList.Values)
        {
            if (!_referee.Enabled.Value) allReady = false;
        }

        if (allReady)
        {
            StartGame();
        }
    }

    void PurchaseUpload(int id, PurchaseType type)
    {

    }
        
    [ServerRpc]
    protected virtual void PurchaseHandlerServerRpc(int id, PurchaseType type, ServerRpcParams serverRpcParams = default)
    {
        RefereeController referee = RefereeControllerList[id];

        int cost = 0;

        switch (type)
        {
            case PurchaseType.Remote_HP:
                cost = 100 + Mathf.CeilToInt((420 - TimeLeft.Value) / 60) * 20;
                if (cost >= Coins.Value[(int)referee.faction.Value]) RemoteHealthSupply(referee);
                break;
            case PurchaseType.Remote_Bullet_17mm:
                cost = 200;
                if (cost >= Coins.Value[(int)referee.faction.Value]) Remote17mmSupply(referee);
                break;
            case PurchaseType.Remote_Bullet_42mm:
                cost = 300;
                if (cost >= Coins.Value[(int)referee.faction.Value]) Remote42mmSupply(referee);
                break;
            default:
                break;
        }
    }

    IEnumerator RemoteHealthSupply(RefereeController robot)
    {
        yield return new WaitForSeconds(RemoteSupplyApplyInterval);

        if (robot.HPLimit.Value * RemoteHPSupllyCount + robot.HP.Value >= robot.HPLimit.Value)
        {
            robot.HP.Value = robot.HPLimit.Value;
        } else {
            robot.HP.Value += robot.HPLimit.Value * 0.6f;
        }
    }

    IEnumerator Remote17mmSupply(RefereeController robot)
    {
        yield return new WaitForSeconds(RemoteSupplyApplyInterval);

        robot.Ammo0.Value += Remote17mmSupplyCount;
    }

    IEnumerator Remote42mmSupply(RefereeController robot)
    {
        yield return new WaitForSeconds(RemoteSupplyApplyInterval);

        robot.Ammo1.Value += Remote42mmSupplyCount;;
    }

    void ResetRemoteSupplyTimes()
    {
        foreach (var fac in Factions)
        {
            RemoteHPTimes.Value[(int)fac] = RemoteHPTimesLimit;
            Remote17mmTimes.Value[(int)fac] = Remote17mmTimesLimit;
            Remote42mmTimes.Value[(int)fac] = Remote42mmTimesLimit;
        }
    }
}
