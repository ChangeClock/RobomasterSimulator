using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class RefereeController : NetworkBehaviour
{
    public delegate void SpawnAction(int robotID);
    public static event SpawnAction OnSpawn;

    public delegate void DamageAction(int damageType, int armorID, int robotID);
    public static event DamageAction OnDamage;

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
    [SerializeField] private TextMeshProUGUI ObserverUI;
    [SerializeField] public NetworkVariable<int> RobotID       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<RobotClass> robotClass = new NetworkVariable<RobotClass>(RobotClass.Infantry);
    [SerializeField] public NetworkVariable<Faction> faction = new NetworkVariable<Faction>(Faction.Neu);

    [Header("Status")]
    // PowerLimit: -1 - Unlimited
    [SerializeField] public NetworkVariable<int> ShieldLimit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Shield            = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> HPLimit           = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> HP                = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> PowerLimit        = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Power             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> EnergyLimit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Energy            = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
   
    // Status
    // Ammo: 0 - 17mm 1 - 42 mm
    [SerializeField] public NetworkVariable<bool> Enabled          = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<bool> Reviving          = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> MaxReviveProgress = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<float> CurrentReviveProgress = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<bool> Immutable         = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Warning           = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> OccupiedArea      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> ATKBuff         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] public NetworkVariable<int> DEFBuff         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] public NetworkVariable<int> CDBuff         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] public NetworkVariable<int> ReviveProgressPerSec = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] public NetworkVariable<int> HealBuff         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Player")]
    private RobotController robotController;

    public override void OnNetworkSpawn()
    {
        // Debug.Log("Client:" + NetworkManager.Singleton.LocalClientId + "IsOwner?" + IsOwner);
        if (IsLocalPlayer) {
            FPV.SetActive(true);
        
            FPVCamera = this.gameObject.GetComponentInChildren<FPVController>();
            FPVCamera.TurnOnCamera();
            if (RobotID.Value < 20)
            {
                FPVCamera.SetColor(Color.red);
            } else {
                FPVCamera.SetColor(Color.blue);
            }

            robotController = this.gameObject.GetComponent<RobotController>();
            robotController.enabled = true;
        }
    }

    void Start()
    {
        if (!IsOwner) return;
        Debug.Log($"[RefereeController] {RobotID.Value} Spawned");
        OnSpawn(RobotID.Value);
    }

    void OnEnable()
    {
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
                _armor.LightColor = RobotID.Value > 20 ? 1 : 2;
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
            LightBar.LightColor = RobotID.Value > 20 ? 1 : 2;
        }

        // Sync Observer UI
        ObserverUI.text = "Player: " + (OwnerClientId) + "\n" + "HP: " + (HP.Value.ToString());
    }

    private void FixedUpdate() {
        if (!IsOwner) return;

        if (robotController != null)
        {
            robotController.enabled = Enabled.Value;
        }
            
        if (FPVCamera != null)
        {
            FPVCamera.Enabled = Enabled.Value;
            FPVCamera.Warning = Warning.Value;
            FPVCamera.SetHPLimit(HPLimit.Value);
            FPVCamera.SetHP(HP.Value);

            FPVCamera.SetHeatLimit(Heat0Limit.Value, Heat1Limit.Value);
            FPVCamera.SetHeat(Heat0.Value, Heat1.Value);

            FPVCamera.SetExpInfo(EXP.Value, EXPToNextLevel.Value);
            FPVCamera.SetLevelInfo(Level.Value);
        }

        TickBuff();
    }

    void DamageHandler(int damageType, int armorID)
    {
        if (OnDamage != null)
        {
            OnDamage(damageType, armorID, RobotID.Value);
        }
    }

    # region Shooter related
    // Shooter Type: 0 - 17mm 1 - 42mm
    // Shooter Mode: 0 - None 1 - Boost 2 - CD 3 - Speed
    [SerializeField] public NetworkVariable<bool> ShooterEnabled = new NetworkVariable<bool>(true);

    // Shooter 0
    [SerializeField] public NetworkVariable<bool> Shooter0Enabled  = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Shooter0Type      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Shooter0Mode      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<float> Heat0Limit        = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<float> Heat0             = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> CD0               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Speed0Limit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
     // Shooter 1
    [SerializeField] public NetworkVariable<bool> Shooter1Enabled   = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Shooter1Type      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Shooter1Mode      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<float> Heat1Limit        = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<float> Heat1             = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> CD1               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Speed1Limit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
 
    // Ammo0 - 17mm
    [SerializeField] public NetworkVariable<int> Ammo0             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> RealAmmo0         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Ammo1 - 42mm
    [SerializeField] public NetworkVariable<int> Ammo1             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> RealAmmo1         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


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

    #endregion

    void DetectHandler(int areaID)
    {
        if (!Enabled.Value) return;

        OnOccupy(areaID, RobotID.Value);
    }

    #region Buff Related
    class BuffEffectInfo
    {
        public BuffEffectSO buffEffect;
        public float timeActivated;
        public BuffEffectInfo(BuffEffectSO buff)
        {
            buffEffect = buff;
            timeActivated = Time.time;
        }
    }

    Dictionary<BuffEffectSO, BuffEffectInfo> activeBuffs = new Dictionary<BuffEffectSO, BuffEffectInfo>();

    [Header("Buff Related")]

    public BuffEffectSO defaultBuff;

    public bool HasBuff(BuffEffectSO buff)
    {
        return activeBuffs.ContainsKey(buff);
    }

    public void AddBuff(BuffEffectSO buff)
    {
        if (activeBuffs.ContainsKey(buff))
        {
            activeBuffs[buff].timeActivated = Time.time;
            return;
        }

        if (buff.DEFBuff > DEFBuff.Value)
        {
            DEFBuff.Value = buff.DEFBuff;
        }

        if (buff.ATKBuff > ATKBuff.Value)
        {
            ATKBuff.Value = buff.ATKBuff;
        }

        if (buff.CDBuff > CDBuff.Value)
        {
            CDBuff.Value = buff.CDBuff;
        }

        // if (buff.speedBoost > speedBoost)
        // {
        //     speedBoost = buff.speedBoost;
        // }

        if (buff.ReviveProgressPerSec > ReviveProgressPerSec.Value)
        {
            ReviveProgressPerSec.Value = buff.ReviveProgressPerSec;
        }

        if (buff.HealBuff > HealBuff.Value)
        {
            HealBuff.Value = buff.HealBuff;
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
            UpdateBuff();
        }
        Debug.Log($"{activeBuffs.Count} buff remains");
    }

    void UpdateBuff()
    {
        BuffEffectSO newBuffStat = Instantiate(defaultBuff);

        if (activeBuffs.Count > 0)
        {
            var activeBuffsCache = activeBuffs.Keys;

            foreach (var activeBuff in activeBuffsCache)
            {
                if (activeBuff.DEFBuff > newBuffStat.DEFBuff)
                {
                    newBuffStat.DEFBuff = activeBuff.DEFBuff;
                }

                if (activeBuff.ATKBuff > newBuffStat.ATKBuff)
                {
                    newBuffStat.ATKBuff = activeBuff.ATKBuff;
                }

                if (activeBuff.CDBuff > newBuffStat.CDBuff)
                {
                    newBuffStat.CDBuff = activeBuff.CDBuff;
                }

                if (activeBuff.speedBoost > newBuffStat.speedBoost)
                {
                    newBuffStat.speedBoost = activeBuff.speedBoost;
                }

                if (activeBuff.ReviveProgressPerSec > newBuffStat.ReviveProgressPerSec)
                {
                    newBuffStat.ReviveProgressPerSec = activeBuff.ReviveProgressPerSec;
                }

                if (activeBuff.HealBuff > newBuffStat.HealBuff)
                {
                    newBuffStat.HealBuff = activeBuff.HealBuff;
                }
            }
        }

        DEFBuff.Value = newBuffStat.DEFBuff;
        ATKBuff.Value = newBuffStat.ATKBuff;
        CDBuff.Value = newBuffStat.CDBuff;
        // speedBoost = newBuffStat.speedBoost;
        ReviveProgressPerSec.Value = newBuffStat.ReviveProgressPerSec;
        HealBuff.Value = newBuffStat.HealBuff;
    }

    void TickBuff()
    {
        if (activeBuffs.Count <= 0)
        {
            return;
        }

        List<BuffEffectSO> overtimeBuffs = new List<BuffEffectSO>();

        var activeBuffCache = activeBuffs.Values;
        foreach (var buffInfo in activeBuffCache)
        {
            if (buffInfo.buffEffect.buffDuration <= 0.0f)
            {
                continue;
            }

            if (buffInfo.timeActivated + buffInfo.buffEffect.buffDuration < Time.time)
            {
                overtimeBuffs.Add(buffInfo.buffEffect);
            }
        }

        if (overtimeBuffs.Count > 0)
        {
            foreach (var overtimeBuff in overtimeBuffs)
            {
                RemoveBuff(overtimeBuff);
            }
        }
    }

    #endregion

    #region EXP related

    public ChassisPerformanceType chassisType;
    public GimbalPerformanceType gimbalType;

    [SerializeField] public NetworkVariable<int> Level             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> EXP               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> EXPToNextLevel    = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> EXPValue          = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<float> TimeToNextEXP = new NetworkVariable<float>(0.0f);


    #endregion
}
