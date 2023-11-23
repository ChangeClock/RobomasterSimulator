using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("Game Status")]
    // 0 - Not started 1 - ready 2 - checking 3 - running 4 - ending
    public NetworkVariable<int> GameStatus = new NetworkVariable<int>(0);
    public NetworkVariable<float> TimeLeft = new NetworkVariable<float>(420.0f);
    public NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false);

    [SerializeField] protected List<Faction> Factions = new List<Faction>(){Faction.Red, Faction.Blue};

    public RefereeController RedBase;
    public RefereeController BlueBase;
    public RefereeController RedOutpost;
    public RefereeController BlueOutpost;
    public RefereeController RedSentry;
    public RefereeController BlueSentry;
    public RefereeController RedLidar;
    public RefereeController BlueLidar;

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
    [SerializeField] private int SentryInitialAmmo = 700;

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
    [SerializeField] private AreaController RedActivateArea;
    [SerializeField] private AreaController BlueActivateArea;

    // [Header("Coin")]
    public int InitialCoin = 400;

    [Header("Purchase")]    
    public NetworkList<int> Coins = new NetworkList<int>();
    public NetworkList<int> CoinsTotal = new NetworkList<int>();
    public NetworkVariable<bool> HasFirstGold = new NetworkVariable<bool>(false);

    [SerializeField] private int RealAmmo0SupplyLimit = 1500;
    public NetworkList<int> RealAmmo0Supply = new NetworkList<int>();
    [SerializeField] private int Ammo0SupplyLimit = 1500;
    public NetworkList<int> Ammo0Supply = new NetworkList<int>();
    [SerializeField] private int Ammo1SupplyLimit = 100;
    public NetworkList<int> Ammo1Supply = new NetworkList<int>();
    
    [SerializeField] private int RemoteSupplyApplyInterval = 6;
    [SerializeField] private int RemoteHPTimesLimit = 2;
    [SerializeField] private float RemoteHPSupplyAmount = 0.6f;
    public NetworkList<int> RemoteHPTimes = new NetworkList<int>();
    [SerializeField] private int RemoteAmmo0TimesLimit = 2;
    [SerializeField] private int RemoteAmmo0SupplyAmount = 100;
    public NetworkList<int> RemoteAmmo0Times = new NetworkList<int>();
    [SerializeField] private int RemoteAmmo1TimesLimit = 2;
    [SerializeField] private int RemoteAmmo1SupplyAmount = 10;
    public NetworkList<int> RemoteAmmo1Times = new NetworkList<int>();

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

        ExchangePoint.OnExchanged += ExchangeUpload;
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

        ExchangePoint.OnExchanged -= ExchangeUpload;
    }

    protected virtual void Start() 
    {
        RedExchangeStation.PriceInfo = PriceInfo;
        BlueExchangeStation.PriceInfo = PriceInfo;

        if (!IsServer) return;

        RefereeControllerList.Add(RedBase.RobotID.Value, RedBase);
        RefereeControllerList.Add(BlueBase.RobotID.Value, BlueBase);
        RefereeControllerList.Add(RedOutpost.RobotID.Value, RedOutpost);
        RefereeControllerList.Add(BlueOutpost.RobotID.Value, BlueOutpost);
        if (RedSentry != null) SpawnUpload(RedSentry.RobotID.Value);
        if (BlueSentry != null) SpawnUpload(BlueSentry.RobotID.Value);
        if (RedLidar != null) RefereeControllerList.Add(RedLidar.RobotID.Value, RedLidar);
        if (BlueLidar != null) RefereeControllerList.Add(BlueLidar.RobotID.Value, BlueLidar);

        if (SentryPerformance != null)
        {
            SetUnitPerformance(RedSentry, 0, SentryPerformance);
            foreach (var _shooter in RedSentry.ShooterControllerList.Values)
            {
                SetShooterPerformance(_shooter, 0, SentryPerformance);
            }
            SetUnitPerformance(BlueSentry, 0, SentryPerformance);
            foreach (var _shooter in BlueSentry.ShooterControllerList.Values)
            {
                SetShooterPerformance(_shooter, 0, SentryPerformance);
            }
        }

        if (SentryExpInfo != null)
        {
            SetUnitEXPInfo(RedSentry, SentryExpInfo);
            SetUnitEXPInfo(BlueSentry, SentryExpInfo);
        }

        foreach(var fac in Factions)
        {
            Coins.Add(InitialCoin);
            CoinsTotal.Add(InitialCoin);
            RealAmmo0Supply.Add(RealAmmo0SupplyLimit);
            Ammo0Supply.Add(Ammo0SupplyLimit);
            Ammo1Supply.Add(Ammo1SupplyLimit);
            RemoteHPTimes.Add(RemoteHPTimesLimit);
            RemoteAmmo0Times.Add(RemoteAmmo0TimesLimit);
            RemoteAmmo1Times.Add(RemoteAmmo1TimesLimit);
        }
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

    public virtual void FinishGame(Faction faction = Faction.Neu)
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

        RedExchangeStation.Reset();
        BlueExchangeStation.Reset();

        ToggleBuff(false, BuffType.Small);
        ToggleActivateArea(false);

        ResetCoin();
        ResetAmmoSupply();
        ResetRemoteSupplyTimes();

        foreach(var _referee in RefereeControllerList.Values)
        {
            _referee.Reset();
        }

        if (RedSentry != null) RedSentry.Ammo0.Value = SentryInitialAmmo;
        if (BlueSentry != null) BlueSentry.Ammo0.Value = SentryInitialAmmo;

        TimeLeft.Value = 420.0f;

        isRunning.Value = true;
    }

    protected void AddCoin(Faction faction, int coin)
    {
        Coins[(int)faction] += coin;
        CoinsTotal[(int)faction] += coin;
    }

    protected void ResetCoin()
    {
        foreach (var fac in Factions)
        {
            Coins[(int)fac] = InitialCoin;
            CoinsTotal[(int)fac] = InitialCoin;
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
        // Debug.Log($"[GameManager] Referee {robotID} SpawnUpload");
        SpawnRefereeServerRpc(robotID);
    }

    [ServerRpc]
    void SpawnRefereeServerRpc(int robotID, ServerRpcParams serverRpcParams = default)
    {
        // Debug.Log($"[GameManager] Referee {robotID} ServerRPC");
        if (RefereeControllerList.ContainsKey(robotID))
        {
            Debug.LogError($"{robotID} exists !!!");
            return;
        }

        RefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None);
        // Debug.Log($"[GameManager] List Length: {_list.Length}");
        
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

    [ServerRpc(RequireOwnership = false)]
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
        // Debug.Log($"[GameManager] mode: {mode}, maxHealth: {performance.maxHealth[referee.Level.Value]}, level: {referee.Level.Value}");
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
    protected virtual void DamageHandlerServerRpc(int damageType, float damage, int armorID, int attackerID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        // Debug.Log("Damage Type: " + damageType + " Armor ID: " + armorID + " Robot ID: " + robotID);
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
    protected virtual void DeathHandlerServerRpc(int attackerID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"Robot {robotID} death event!");

        RefereeController Victim = RefereeControllerList[robotID];

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
                Victim.MaxReviveProgress.Value = (int)Math.Round(10.0f + (420.0f - TimeLeft.Value) / 10.0f);
                Victim.PurchaseRevivePrice.Value = (int)Math.Round(((420 - TimeLeft.Value) / 60) * 100 + (Victim.Level.Value + 1) * 50);
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

    protected void DistributeEXP(Faction faction, float exp)
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
                Coins[(int)RefereeControllerList[id].faction.Value] -= RefereeControllerList[id].PurchaseRevivePrice.Value;
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

    void PurchaseUpload(int id, PurchaseType type, int amount)
    {
        PurchaseHandlerServerRpc(id, type, amount);
    }
        
    [ServerRpc]
    protected virtual void PurchaseHandlerServerRpc(int id, PurchaseType type, int amount, ServerRpcParams serverRpcParams = default)
    {
        // Debug.Log($"[GameManager] id {id} type {type}, amount {amount}");

        RefereeController referee = RefereeControllerList[id];
        Faction faction = referee.faction.Value;

        int cost = 0;

        // TODO #37
        switch (type)
        {
            case PurchaseType.Remote_HP:
                cost = 100 + Mathf.CeilToInt((420 - TimeLeft.Value) / 60) * 20;
                if (CostCoin(faction, cost)) 
                {
                    RemoteHPTimes[(int)faction] --;
                    StartCoroutine(RemoteHealthSupply(referee));
                }
                break;
            case PurchaseType.Remote_Ammo0:
                cost = 200;
                if (CostCoin(faction, cost)) 
                {
                    RemoteAmmo0Times[(int)faction] --;
                    StartCoroutine(RemoteAmmo0Supply(referee));
                }
                break;
            case PurchaseType.Remote_Ammo1:
                cost = 300;
                if (CostCoin(faction, cost)) 
                {
                    RemoteAmmo1Times[(int)faction] --;
                    StartCoroutine(RemoteAmmo1Supply(referee));
                }
                break;
            case PurchaseType.Ammo0:
                cost = amount;
                if (CostCoin(faction, cost)) 
                {
                    Ammo0Supply[(int)faction] -= amount;
                    referee.Ammo0.Value += amount;
                }
                break;
            case PurchaseType.Ammo1:
                cost = amount * 15;
                if (CostCoin(faction, cost)) 
                {
                    Ammo1Supply[(int)faction] -= amount;
                    referee.Ammo1.Value += amount;
                }
                break;
            default:
                break;
        }
    }

    protected bool CostCoin(Faction faction, int cost)
    {
        if (Coins[(int)faction] >= cost)
        {
            Coins[(int)faction] -= cost;
            return true;
        }
        return false;
    }

    protected IEnumerator RemoteHealthSupply(RefereeController robot)
    {
        yield return new WaitForSeconds(RemoteSupplyApplyInterval);

        if (!robot.Enabled.Value) yield break;

        if (robot.HPLimit.Value * RemoteHPSupplyAmount + robot.HP.Value >= robot.HPLimit.Value)
        {
            robot.HP.Value = robot.HPLimit.Value;
        } else {
            robot.HP.Value += robot.HPLimit.Value * 0.6f;
        }
    }

    protected IEnumerator RemoteAmmo0Supply(RefereeController robot)
    {
        yield return new WaitForSeconds(RemoteSupplyApplyInterval);

        // Debug.Log($"[GameManager] Remote Ammo0 Supply {RemoteAmmo0SupplyAmount}");
        robot.Ammo0.Value += RemoteAmmo0SupplyAmount;
    }

    protected IEnumerator RemoteAmmo1Supply(RefereeController robot)
    {
        yield return new WaitForSeconds(RemoteSupplyApplyInterval);

        robot.Ammo1.Value += RemoteAmmo1SupplyAmount;
    }

    void ResetAmmoSupply()
    {
        foreach (var fac in Factions)
        {
            Ammo0Supply[(int)fac] = Ammo0SupplyLimit;
            Ammo1Supply[(int)fac] = Ammo0SupplyLimit;
        }
    }

    void ResetRemoteSupplyTimes()
    {
        foreach (var fac in Factions)
        {
            RemoteHPTimes[(int)fac] = RemoteHPTimesLimit;
            RemoteAmmo0Times[(int)fac] = RemoteAmmo0TimesLimit;
            RemoteAmmo1Times[(int)fac] = RemoteAmmo1TimesLimit;
        }
    }

    #region Buff Devices

    public delegate void BuffActivedAction(Faction faction, BuffType type, int totalScore = 0, BuffEffectSO effect = null);
    public static event BuffActivedAction OnBuffActived;
    
    public BuffController RedBuffDevice;
    public BuffController BlueBuffDevice;

    protected void ToggleBuff(bool enable, BuffType type)
    {
        int direction =  UnityEngine.Random.Range(-1,1);

        RedBuffDevice.Toggle(enable, direction, type);
        BlueBuffDevice.Toggle(enable, direction, type);
    }

    protected void ToggleActivateArea(bool enable)
    {
        RedActivateArea.Reset();
        BlueActivateArea.Reset();
        RedActivateArea.Enabled.Value = enable;
        BlueActivateArea.Enabled.Value = enable;
    }

    protected void AddFactionBuff(Faction faction, BuffEffectSO buff)
    {
        foreach(var referee in RefereeControllerList.Values)
        {
            if (referee.faction.Value == faction & !referee.robotTags.Contains(RobotTag.Building)) referee.AddBuff(buff);
        }
    }

    protected void BuffActivatedEvent(Faction faction, BuffType type, int totalScore = 0, BuffEffectSO effect = null)
    {
        if (OnBuffActived != null) OnBuffActived(faction, type, totalScore, effect);
    }

    #endregion
    
    #region Exchange Staion

    public ExchangePriceSO PriceInfo;

    public ExchangePoint RedExchangeStation;
    public ExchangePoint BlueExchangeStation;

    
    void ExchangeUpload(Faction faction, OreType type, int value)
    {
        ExchangeHandlerServerRpc(faction, type, value);
    }

    [ServerRpc]
    void ExchangeHandlerServerRpc(Faction faction, OreType type, int value, ServerRpcParams serverRpcParams = default)
    {
        int coin = value;

        if (type == OreType.Gold && !HasFirstGold.Value)
        {
            coin += 250;
            HasFirstGold.Value = true;
        }

        AddCoin(faction, coin);
    }

    #endregion
}