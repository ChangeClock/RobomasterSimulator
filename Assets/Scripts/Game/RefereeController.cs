using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

[System.Serializable]
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

    public DataTransmission.RobotStatus Status = new DataTransmission.RobotStatus();

    [SerializeField] private TextMeshProUGUI ObserverUI;

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

    // Shooter Type: 0 - 17mm 1 - 42mm
    // Shooter Mode: 0 - None 1 - Boost 2 - CD 3 - Speed
    // Shooter 0
    [SerializeField] public NetworkVariable<bool> Shooter0Enabled  = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Shooter0Type      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Shooter0Mode      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Heat0Limit        = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Heat0             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> CD0               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Speed0Limit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // Shooter 1
    [SerializeField] public NetworkVariable<bool> Shooter1Enabled   = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Shooter1Type      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Shooter1Mode      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Heat1Limit        = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Heat1             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> CD1               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Speed1Limit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    // Status
    // Ammo: 0 - 17mm 1 - 42 mm
    [SerializeField] public NetworkVariable<int> Ammo0             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> RealAmmo0         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Ammo1             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> RealAmmo1         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<bool> Enabled          = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<bool> Immutable         = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Level             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> EXP               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> EXPValue               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Warning           = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> OccupiedArea      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> ATKBuff      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> DEFBuff      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> CDBuff      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Game Info
    [SerializeField] public NetworkVariable<int> TimePast               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> RBaseShieldLimit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> RBaseShield            = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> RBaseHPLimit           = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> RBaseHP                = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> BBaseShieldLimit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> BBaseShield            = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> BBaseHPLimit           = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> BBaseHP                = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> ROutpostHPLimit           = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> ROutpostHP                = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> BOutpostHPLimit           = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> BOutpostHP                = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Referee")]
    private ArmorController[] Armors;
    private ShooterController[] Shooters;
    private RFIDController RFID;
    private LightbarController LightBar;
    private CameraController Camera;
    public int RobotID;

    public override void OnNetworkSpawn()
    {

    }

    void Start()
    {
        Debug.Log($"[RefereeController] {RobotID} Spawned");
        OnSpawn(RobotID);
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

        Camera = this.gameObject.GetComponentInChildren<CameraController>();

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
                _armor.LightColor = RobotID > 20 ? 1 : 2;
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

        if (Camera != null)
        {
            Camera.Enabled = Enabled.Value;
            Camera.Warning = Warning.Value;
            Camera.HPLimit = HPLimit.Value;
            Camera.HP = HP.Value;
            Camera.TimePast = TimePast.Value;    
            Camera.RBaseShieldLimit = RBaseShieldLimit.Value;
            Camera.RBaseShield     = RBaseShield.Value; 
            Camera.RBaseHPLimit    = RBaseHPLimit.Value;
            Camera.RBaseHP       = RBaseHP.Value;  
            Camera.BBaseShieldLimit = BBaseShieldLimit.Value;
            Camera.BBaseShield    = BBaseShield.Value;  
            Camera.BBaseHPLimit    = BBaseHPLimit.Value;
            Camera.BBaseHP         = BBaseHP.Value;
            Camera.ROutpostHPLimit = ROutpostHPLimit.Value;
            Camera.ROutpostHP      = ROutpostHP.Value;
            Camera.BOutpostHPLimit = BOutpostHPLimit.Value;
            Camera.BOutpostHP      = BOutpostHP.Value;
        }

        // TODO: Need to sync the HP status
        if (LightBar != null)
        {
            LightBar.Enabled = Enabled.Value;
            LightBar.Warning = Warning.Value;
            LightBar.LightColor = RobotID > 20 ? 1 : 2;
        }

        // Sync Observer UI
        ObserverUI.text = "Player: " + (OwnerClientId) + "\n" + "HP: " + (HP.Value.ToString());
    }

    void DamageHandler(int damageType, int armorID)
    {
        if (OnDamage != null)
        {
            OnDamage(damageType, armorID, RobotID);
        }
    }

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
        OnShoot(ID, ID == 0 ? Shooter0Type.Value : Shooter1Type.Value, RobotID, Position, Velocity);
    }

    void DetectHandler(int areaID)
    {
        if (!Enabled.Value) return;

        OnOccupy(areaID, RobotID);
    }
}
