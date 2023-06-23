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
    [SerializeField] private NetworkVariable<int> ShieldLimit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Shield            = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> HPLimit           = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> HP                = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> PowerLimit        = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Power             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> EnergyLimit            = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Energy            = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Shooter Type: 0 - 17mm 1 - 42mm
    // Shooter Mode: 0 - None 1 - Boost 2 - CD 3 - Speed
    // Shooter 0
    [SerializeField] private NetworkVariable<bool> Shooter0Enabled  = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Shooter0Type      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Shooter0Mode      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Heat0Limit        = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Heat0             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> CD0               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Speed0Limit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // Shooter 1
    [SerializeField] private NetworkVariable<int> Shooter1Enabled   = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Shooter1Type      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Shooter1Mode      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Heat1Limit        = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Heat1             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> CD1               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Speed1Limit       = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    // Status
    // Ammo: 0 - 17mm 1 - 42 mm
    [SerializeField] private NetworkVariable<int> Ammo0             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> RealAmmo0         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Ammo1             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> RealAmmo1         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Disabled          = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Immutable         = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Level             = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> EXP               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> EXPValue               = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Warning           = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> OccupiedArea      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> ATKBuff      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> DEFBuff      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> CDBuff      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> Time      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Referee")]
    private ArmorController[] Armors;
    private ShooterController[] Shooters;
    private RFIDController RFID;
    private LightbarController LightBar;
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
                _armor.disabled = Disabled.Value != 0;
                _armor.lightColor = RobotID > 20 ? 1 : 2;
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
                        _shooter.SetEnabled(Shooter1Enabled.Value != 0);
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
            LightBar.disabled = Disabled.Value != 0;
            LightBar.warning = Warning.Value != 0;
            LightBar.lightColor = RobotID > 20 ? 1 : 2;
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
        if (Disabled.Value == 1) return;

        // Judge whether there is free ammo, both real ammo and ammo in referee system
        if (ID == 0 && (Ammo0.Value <= 0 || RealAmmo0.Value <= 0)) return;
        if (ID == 1 && (Ammo1.Value <= 0 || RealAmmo1.Value <= 0)) return;

        // Debug.Log("[RefereeController] OnShoot");

        // Free Fire!
        OnShoot(ID, ID == 0 ? Shooter0Type.Value : Shooter1Type.Value, RobotID, Position, Velocity);
    }

    void DetectHandler(int areaID)
    {
        if (Disabled.Value == 1) return;

        OnOccupy(areaID, RobotID);
    }

    public void SetHP(int hp)
    {
        HP.Value = hp;
    }

    public int GetHP()
    {
        return HP.Value;
    }

    public void SetDisabled(bool disabled)
    {
        Disabled.Value = disabled ? 1 : 0;
    }

    public bool GetDisabled()
    {
        return Disabled.Value == 1 ? true : false;
    }

    public void SetImmutable(bool immutable)
    {
        Immutable.Value = immutable ? 1 : 0;
    }

    public bool GetImmutable()
    {
        return Immutable.Value == 1 ? true : false;
    }
}
