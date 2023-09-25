using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class RefereeController : NetworkBehaviour
{
    private GameManager gameManager;

    public delegate void SpawnAction(int robotID);
    public static event SpawnAction OnSpawn;

    public delegate void DamageAction(int damageType, float damage, int armorID, int attackerID,int robotID);
    public static event DamageAction OnDamage;

    // mode: 0-自然复活 1-买活
    public delegate void RevivedAction(int id, int mode = 0);
    public static event RevivedAction OnRevived;

    public delegate void DeathAction(int attackerID, int id);
    public static event DeathAction OnDeath;

    public delegate void ShootAction(int shooterID, int shooterType, int robotID, Vector3 userPosition, Vector3 shootVelocity);
    public static event ShootAction OnShoot;

    public delegate void OccupyAction(int areaID, int robotID);
    public static event OccupyAction OnOccupy;

    // public DataTransmission.RobotStatus Status = new DataTransmission.RobotStatus();

    [Header("Referee")]
    [SerializeField]private GameObject FPV;
    private ArmorController[] Armors;
    private ShooterController[] Shooters;
    private RFIDController RFID;
    private LightbarController LightBar;
    private FPVController FPVCamera;
    private EnergyController EnergyCtl;

    public NetworkVariable<int> RobotID       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<RobotClass> robotClass = new NetworkVariable<RobotClass>(RobotClass.Infantry);
    public List<RobotTag> robotTags = new List<RobotTag>();
    public NetworkVariable<Faction> faction = new NetworkVariable<Faction>(Faction.Neu);

    [Header("Player")]
    private RobotController robotController;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            EXPToNextLevel.Value = EXPInfo.expToNextLevel[Level.Value];
            EXPValue.Value = EXPInfo.expValue[Level.Value];
        }

        // Debug.Log("Client:" + NetworkManager.Singleton.LocalClientId + "IsOwner?" + IsOwner);
        if (IsOwner) 
        {
            FPV.SetActive(true);

            robotController = this.gameObject.GetComponent<RobotController>();
            robotController.Enabled = true;
        
            FPVCamera = this.gameObject.GetComponentInChildren<FPVController>();
            
            FPVCamera.SetShooterInfo(Shooter0Enabled.Value, Shooter1Enabled.Value);
            
            FPVCamera.TurnOnCamera();

            FPVCamera.SetRoleInfo(faction.Value, RobotID.Value);
        }
    }

    void Start()
    {
        if (!IsServer) return;
        Debug.Log($"[RefereeController] {RobotID.Value} Spawned");
        OnSpawn(RobotID.Value);
    }

    void OnEnable()
    {
        gameManager = GameObject.FindAnyObjectByType<GameManager>();

        Armors = this.gameObject.GetComponentsInChildren<ArmorController>();
        foreach(ArmorController _armor in Armors)
        {
            if(_armor != null) 
            {
                _armor.OnHit += DamageHandler;
            }
        }

        Shooters = this.gameObject.GetComponentsInChildren<ShooterController>();
        foreach(ShooterController _shooter in Shooters)
        {
            if(_shooter != null)
            {
                _shooter.OnTrigger += TriggerHandler;
            }
        }

        EnergyCtl = this.gameObject.GetComponent<EnergyController>();
        if (EnergyCtl != null)
        {
            EnergyCtl.SetMaxPower(PowerLimit.Value);
            EnergyCtl.SetMaxBuffer(BufferLimit.Value);
        }

        RFID = this.gameObject.GetComponentInChildren<RFIDController>();
        if (RFID != null) RFID.OnDetect += DetectHandler;

        LightBar = this.gameObject.GetComponentInChildren<LightbarController>();
    }

    void OnDisable()
    {
        // Debug.Log("Disable Armors: "+ Armors.Length);
        foreach(ArmorController _armor in Armors)
        {
            _armor.OnHit -= DamageHandler;
        }

        foreach(ShooterController _shooter in Shooters)
        {
            if(_shooter != null)
            {
                _shooter.OnTrigger -= TriggerHandler;
            }
        }
        
        if (RFID != null) RFID.OnDetect -= DetectHandler;
    }

    void Update()
    {
        // Sync Armor related Status
        foreach(ArmorController _armor in Armors)
        {
            if(_armor != null) 
            {
                _armor.Enabled = Enabled.Value;
                _armor.LightColor = faction.Value == Faction.Red ? 1 : 2;
            }
        }

        // TODO: Need to sync the heat
        int _counter = 0;
        foreach(ShooterController _shooter in Shooters)
        {
            if(_shooter != null)
            {
                switch(_counter)
                {
                    case 0:
                        _shooter.SetEnabled(Shooter0Enabled.Value);
                        break;
                    case 1:
                        _shooter.SetEnabled(Shooter1Enabled.Value);
                        break;
                    default:
                        Debug.LogError("Unknown shooter ID");
                        break;
                }
            }
        }

        // TODO: need to sync the disable status to control the light
        if (RFID != null)
        {

        }

        // TODO: Need to sync the HP status
        if (LightBar != null)
        {
            LightBar.Enabled = Enabled.Value;
            LightBar.Warning = Warning.Value;
            LightBar.LightColor = faction.Value == Faction.Red ? 1 : 2;
        }
    }

    private void FixedUpdate() 
    {
        if (IsOwner)
        {
            if (robotController != null)
            {
                robotController.Enabled = Enabled.Value;
            }
                
            if (FPVCamera != null)
            {
                FPVCamera.Enabled = Enabled.Value;
                FPVCamera.Warning = Warning.Value;

                FPVCamera.SetRoleInfo(faction.Value, RobotID.Value);

                FPVCamera.SetHPLimit(HPLimit.Value);
                FPVCamera.SetHP(HP.Value);

                FPVCamera.SetHeatLimit(Heat0Limit.Value, Heat1Limit.Value);
                FPVCamera.SetHeat(Heat0.Value, Heat1.Value);
                FPVCamera.SetAmmo(Shooter0Type.Value == 0? ConsumedAmmo0.Value : ConsumedAmmo1.Value, Shooter0Type.Value == 0? Ammo0.Value : Ammo1.Value, Shooter1Type.Value == 0? ConsumedAmmo0.Value : ConsumedAmmo1.Value, Shooter1Type.Value == 0? Ammo0.Value : Ammo1.Value);

                FPVCamera.SetHealBuff(HealBuff.Value > 0, HealBuff.Value);
                FPVCamera.SetDEFBuff(DEFBuff.Value > 0, DEFBuff.Value);
                FPVCamera.SetATKBuff(ATKBuff.Value > 0, ATKBuff.Value);
                FPVCamera.SetCDBuff(CDBuff.Value > 0, CDBuff.Value);

                FPVCamera.SetExpInfo(EXP.Value, EXPToNextLevel.Value);
                FPVCamera.SetLevelInfo(Level.Value);

                if (PowerLimit.Value >= 0)
                {
                    FPVCamera.SetPower(Power.Value, IsOverPower ? 1 : 0);
                } else {
                    FPVCamera.SetPower(Power.Value);
                }

                FPVCamera.SetMaxBuffer(EnergyCtl.GetMaxBuffer());
                FPVCamera.SetBuffer(EnergyCtl.GetBuffer());
            }
        }

        if (IsServer)
        {
            if (EnergyCtl != null) TickPower();

            if (Reviving.Value) TickRevive();

            TickHeat();

            TickBuff();

            if (!Enabled.Value) return;

            if (EXPInfo != null) TickEXP();

            TickHealth();
        }

        // TickBuffServerRpc();
    }
    
    [Header("Status")]
    // PowerLimit: -1 - Unlimited
    public NetworkVariable<int> ShieldLimit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Shield            = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> HPLimit           = new NetworkVariable<float>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> HP                = new NetworkVariable<float>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> PowerLimit        = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> Power             = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> BufferLimit       = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> Buffer            = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public class AttackerInfo {
        public int ID;
        public float lastTime;
        public float liveTime = 5.0f;
        public AttackerInfo(int attackerID)
        {
            ID = attackerID;
            lastTime = liveTime;
        }

        public void resetLastTime()
        {
            lastTime = liveTime;
        }
    }

    public Dictionary<int, AttackerInfo> AttackList = new Dictionary<int, AttackerInfo>();

    bool IsOverPower 
    {
        get { return Power.Value > PowerLimit.Value; }
    }
    
    void TickPower()
    {
        Power.Value = EnergyCtl.GetPower();
    }

    void DamageHandler(int damageType, float damage, int armorID, int attackerID)
    {
        if (!IsServer) return;

        if (AttackList.ContainsKey(attackerID)) 
        {
            AttackList[attackerID].resetLastTime();
        } else {
            AttackList.Add(attackerID, new AttackerInfo(attackerID));
        }

        float _hp = HP.Value;
        float _damage = damage;

        if (Enabled.Value && !Immutable.Value) {
            // Debug.Log("[GameManager - Damage] HP:"+RobotStatusList[robotID].HP);
            switch(damageType){
                case 0:
                case 1:
                case 2:
                    // TODO: 42mm sniper doesn't get affected by atkBuff
                    int atkBuff = gameManager.RefereeControllerList[attackerID].ATKBuff.Value;
                    if (atkBuff > 0) _damage = _damage * atkBuff;

                    break;
                case 3:
                    // Missle damage doesn't get affected by atkbuff
                    break;
                default:
                    Debug.LogWarning("Unknown Damage Type" + damageType);
                    break;
            }

            if (DEFBuff.Value > 0) _damage = _damage * (1 - DEFBuff.Value / 100);

            if (_hp - _damage <= 0)
            {
                HP.Value = 0;
                Enabled.Value = false;
                OnDeath(attackerID, RobotID.Value);
            } else {
                HP.Value = (_hp - _damage);
            }
        }

        if (OnDamage != null)
        {
            OnDamage(damageType, _damage, armorID, attackerID, RobotID.Value);
        }
    }

    void TickAttacker()
    {
        if (AttackList.Count <= 0) return;

        List<int> overtimeAttackers = new List<int>();

        foreach (var _info in AttackList.Values)
        {
            _info.lastTime -= Time.deltaTime;
            if (_info.lastTime <= 0)
            {
                overtimeAttackers.Add(_info.ID);
            }
        }

        if (overtimeAttackers.Count <= 0) return;

        foreach (var _id in overtimeAttackers)
        {
            AttackList.Remove(_id);
        }
    }

    void TickHealth()
    {
        if (HP.Value < HPLimit.Value && HealBuff.Value > 0)
        {
            float _recoverHP = HPLimit.Value * HealBuff.Value / 100 * Time.deltaTime;

            if (HP.Value + _recoverHP < HPLimit.Value)
            {
                HP.Value += _recoverHP;
            } else {
                HP.Value = HPLimit.Value;
            }
        }
    }

    void TickRevive()
    {
        Debug.Log($"[RefereeController] Revive Progress {CurrentReviveProgress.Value} / {MaxReviveProgress.Value}");

        CurrentReviveProgress.Value += ReviveProgressPerSec.Value * Time.deltaTime;

        if (CurrentReviveProgress.Value >= MaxReviveProgress.Value)
        {
            Debug.Log($"Robot {RobotID.Value} revived!");
            Reviving.Value = false;
            CurrentReviveProgress.Value = 0;
            HP.Value = HPLimit.Value * 10 / 100;
            Enabled.Value = true;

            OnRevived(RobotID.Value);
            // AddBuff(ReviveBuff);
        }
    }

    void RefereeDamage(float damage)
    {
        if (Enabled.Value)
        {
            float _hp = HP.Value;
            
            // Debug.Log($"[GameManager] raw damage: {damage}, damage {damage}");

            if (_hp - damage <= 0)
            {
                HP.Value = 0;
                Enabled.Value = false;
                OnDeath(RobotID.Value, RobotID.Value);
            } else {
                HP.Value = (_hp - damage);
            }
        }
    }

    public virtual void ShieldOff()
    {
        if (!IsServer) return;

        Shield.Value = 0;
    }

    // Status
    public NetworkVariable<bool> Enabled          = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> Reviving          = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> MaxReviveProgress = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> ReviveProgressPerSec = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> CurrentReviveProgress = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> Immutable         = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Warning           = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> OccupiedArea      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    # region Shooter related
    // Shooter Type: 0 - 17mm 1 - 42mm
    // Shooter Mode: 0 - None 1 - Boost 2 - CD 3 - Speed
    public NetworkVariable<bool> ShooterEnabled = new NetworkVariable<bool>(true);

    // Shooter 0
    public NetworkVariable<bool> Shooter0Enabled  = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Shooter0Type      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Shooter0Mode      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> Heat0Limit        = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> Heat0             = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> CD0               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Speed0Limit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
     // Shooter 1
    public NetworkVariable<bool> Shooter1Enabled   = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Shooter1Type      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Shooter1Mode      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> Heat1Limit        = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> Heat1             = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> CD1               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Speed1Limit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Ammo: 0 - 17mm 1 - 42 mm
    // Ammo0 - 17mm
    public NetworkVariable<int> ConsumedAmmo0     = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Ammo0             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> RealAmmo0         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Ammo1 - 42mm
    public NetworkVariable<int> ConsumedAmmo1     = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Ammo1             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> RealAmmo1         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    void TriggerHandler(int ID, Vector3 Position, Vector3 Velocity)
    {
        // TODO: handle trigger events

        // First judge whether the unit is disabled or not
        if (!Enabled.Value) return;

        // Judge whether there is free ammo, both real ammo and ammo in referee system
        if (ID == 0)
        {
            if (Shooter0Type.Value == 0 && (Ammo0.Value <= 0 || RealAmmo0.Value <= 0)) return;
            if (Shooter0Type.Value == 1 && (Ammo1.Value <= 0 || RealAmmo1.Value <= 0)) return;
        }

        if (ID == 1)
        {
            if (Shooter1Type.Value == 0 && (Ammo0.Value <= 0 || RealAmmo0.Value <= 0)) return;
            if (Shooter1Type.Value == 1 && (Ammo1.Value <= 0 || RealAmmo1.Value <= 0)) return;
        }

        // Debug.Log("[RefereeController] OnShoot");

        // Free Fire!
        OnShoot(ID, ID == 0 ? Shooter0Type.Value : Shooter1Type.Value, RobotID.Value, Position, Velocity);
    }

    void TickHeat()
    {
        int CDFactor = 1;
        if (CDBuff.Value > 0) CDFactor = CDBuff.Value;

        if (Shooter0Enabled.Value & Heat0.Value > 0)
        {
            // Shooter 0 overheating
            if (Heat0.Value > Heat0Limit.Value & Heat0.Value <= 2*Heat0Limit.Value)
            {
                RefereeDamage((Heat0.Value - Heat0Limit.Value) / 250 / 10 * HPLimit.Value);
            } else if (Heat0.Value >= 2*Heat0Limit.Value) {
                RefereeDamage((Heat0.Value - 2*Heat0Limit.Value) / 250 * HPLimit.Value);
                Heat0.Value = 2*Heat0Limit.Value;
            }
            
            // Shooter 0 CD
            if (Heat0.Value >= CD0.Value * CDFactor * Time.deltaTime) 
            {
                Heat0.Value -= CD0.Value * CDFactor * Time.deltaTime;
            } else {
                Heat0.Value = 0;
            }
        }

        if (Shooter1Enabled.Value & Heat1.Value > 0)
        {
            // Shooter 1 overheating
            if (Heat1.Value > Heat1Limit.Value & Heat1.Value <= 2*Heat1Limit.Value)
            {
                RefereeDamage((Heat1.Value - Heat1Limit.Value) / 250 / 10 * HPLimit.Value);
            } else if (Heat1.Value >= 2*Heat1Limit.Value) {
                RefereeDamage((Heat1.Value - 2*Heat1Limit.Value) / 250 * HPLimit.Value);
                Heat1.Value = 2*Heat1Limit.Value;
            }

            if (Heat1.Value >= CD1.Value * CDFactor * Time.deltaTime) 
            {
                Heat1.Value -= CD1.Value * CDFactor * Time.deltaTime;
            } else {
                Heat1.Value = 0;
            }
        }
    }

    #endregion

    #region Buff Related
    
    [Header("Buff Related")]
    
    public NetworkVariable<int> ATKBuff         = new NetworkVariable<int>(0);
    public NetworkVariable<int> DEFBuff         = new NetworkVariable<int>(0);
    public NetworkVariable<int> CDBuff         = new NetworkVariable<int>(0);
    public NetworkVariable<int> HealBuff         = new NetworkVariable<int>(0);

    public class BuffEffectInfo
    {
        public BuffEffectSO buffEffect;
        public float lastTime;
        public BuffEffectInfo(BuffEffectSO buff)
        {
            buffEffect = buff;
            lastTime = 0.0f;
        }
    }

    public Dictionary<BuffEffectSO, BuffEffectInfo> activeBuffs = new Dictionary<BuffEffectSO, BuffEffectInfo>();

    public BuffEffectSO defaultBuff;

    public bool HasBuff(BuffEffectSO buff)
    {
        return activeBuffs.ContainsKey(buff);
    }

    public void AddBuff(BuffEffectSO buff)
    {
        if (activeBuffs.ContainsKey(buff))
        {
            activeBuffs[buff].lastTime = 0.0f;
            return;
        }

        Debug.Log($"Adding {buff.name}");
        activeBuffs.Add(buff, new BuffEffectInfo(buff));
    }

    public void RemoveBuff(BuffEffectSO buff)
    {
        if (activeBuffs.ContainsKey(buff))
        {
            Debug.Log($"Removing {buff.name}");
            activeBuffs.Remove(buff);
        }
        Debug.Log($"{activeBuffs.Count} buff remains");
    }

    void TickBuff()
    {
        if (defaultBuff == null) return;

        BuffEffectSO newBuffStat = Instantiate(defaultBuff);

        if (activeBuffs.Count > 0)
        {
            var _activeBuffsCache = activeBuffs.Values;
            List<BuffEffectSO> overtimeBuff = new List<BuffEffectSO>();

            foreach (var _buffInfo in _activeBuffsCache)
            {
                var _buff = _buffInfo.buffEffect;

                if (_buff.DEFBuff > newBuffStat.DEFBuff)
                {
                    newBuffStat.DEFBuff = _buff.DEFBuff;
                }

                if (_buff.ATKBuff > newBuffStat.ATKBuff)
                {
                    newBuffStat.ATKBuff = _buff.ATKBuff;
                }

                if (_buff.CDBuff > newBuffStat.CDBuff)
                {
                    newBuffStat.CDBuff = _buff.CDBuff;
                }

                if (_buff.ReviveProgressPerSec > newBuffStat.ReviveProgressPerSec)
                {
                    newBuffStat.ReviveProgressPerSec = _buff.ReviveProgressPerSec;
                }

                if (_buff.HealBuff > newBuffStat.HealBuff)
                {
                    newBuffStat.HealBuff = _buff.HealBuff;
                }

                _buffInfo.lastTime += Time.deltaTime;
                if (_buffInfo.lastTime > _buff.buffDuration) overtimeBuff.Add(_buff);
            }

            if (overtimeBuff.Count > 0)
            {
                foreach(var _buff in overtimeBuff)
                {
                    RemoveBuff(_buff);
                }
            }
        }

        ReviveProgressPerSec.Value = newBuffStat.ReviveProgressPerSec;

        if (!Enabled.Value) return; 

        HealBuff.Value = newBuffStat.HealBuff;
        DEFBuff.Value = newBuffStat.DEFBuff;
        ATKBuff.Value = newBuffStat.ATKBuff;
        CDBuff.Value = newBuffStat.CDBuff;
    }

    void DetectHandler(int areaID)
    {
        if (!Enabled.Value) return;

        OnOccupy(areaID, RobotID.Value);
    }

    #endregion

    #region EXP related

    public NetworkVariable<int> Level             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> EXP               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> EXPToNextLevel    = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> EXPValue          = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> TimeToNextEXP = new NetworkVariable<float>(0.0f);

    public RobotPerformanceSO ChassisPerformance;
    public RobotPerformanceSO GimbalPerformance0;
    public RobotPerformanceSO GimbalPerformance1;

    public ExpInfoSO EXPInfo;

    void TickEXP()
    {        
        // EXP growth with time
        TimeToNextEXP.Value += Time.deltaTime;

        if (TimeToNextEXP.Value >= EXPInfo.expGrowth)
        {
            TimeToNextEXP.Value -= EXPInfo.expGrowth;
            EXP.Value += 1;
        }
        
        // Add up EXP but don't level up if the performance are not chosen yet
        if (ChassisPerformance == null || GimbalPerformance0 == null) return;
        if (Shooter1Enabled.Value && GimbalPerformance1 == null) return;

        // Level up
        if (EXP.Value >= EXPInfo.expToNextLevel[Level.Value] && EXPInfo.expToNextLevel[Level.Value] >= 0)
        {
            // Don't zero the current EXP, just minus the EXP needed to next level
            EXPToNextLevel.Value = EXPInfo.expToNextLevel[Level.Value];
            EXP.Value -= EXPInfo.expToNextLevel[Level.Value];
            Level.Value += 1;

            EXPValue.Value = EXPInfo.expValue[Level.Value];

            float _recoverHP = ChassisPerformance.maxHealth[Level.Value] - HPLimit.Value;
            HPLimit.Value = ChassisPerformance.maxHealth[Level.Value];
            HP.Value += _recoverHP;
            PowerLimit.Value = ChassisPerformance.maxPower[Level.Value];

            Heat0Limit.Value = GimbalPerformance0.maxHeat[Level.Value];
            CD0.Value = GimbalPerformance0.coolDown[Level.Value];
            Speed0Limit.Value = GimbalPerformance0.shootSpeed[Level.Value];

            if (Shooter1Enabled.Value)
            {
                Heat1Limit.Value = GimbalPerformance1.maxHeat[Level.Value];
                CD1.Value = GimbalPerformance1.coolDown[Level.Value];
                Speed1Limit.Value = GimbalPerformance1.shootSpeed[Level.Value];
            }
        }
    }

    #endregion
}
