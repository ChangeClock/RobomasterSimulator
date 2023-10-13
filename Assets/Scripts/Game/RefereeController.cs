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

    // public delegate void LevelupAction(int robotID);
    // public static event LevelupAction OnLevelup;

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

    public delegate void ReadyAction(int robotID);
    public static event ReadyAction OnReady;

    public delegate void ChangePerformanceAction(int robotID, int chassisMode, int shooter1Mode, int shooter2Mode);
    public static event ChangePerformanceAction OnPerformanceChange;

    public delegate void PurchaseAction(int robotID, PurchaseType type, int amount = 1);
    public static event PurchaseAction OnPurchase;

    // public DataTransmission.RobotStatus Status = new DataTransmission.RobotStatus();

    [Header("Referee")]
    private ArmorController[] Armors;
    public Dictionary<int, ShooterController> ShooterControllerList = new Dictionary<int, ShooterController>();
    private RFIDController RFID;
    private LightbarController LightBar;
    private UWBController UWB;
    private FPVController FPVCamera;
    // private EnergyController EnergyCtl;
    private WheelController[] Wheels;

    public NetworkVariable<int> RobotID = new NetworkVariable<int>(0);
    public NetworkVariable<RobotClass> robotClass = new NetworkVariable<RobotClass>(RobotClass.Infantry);
    public List<RobotTag> robotTags = new List<RobotTag>();
    public NetworkVariable<Faction> faction = new NetworkVariable<Faction>(Faction.Neu);
    
    [Header("Player")]
    public Transform spawnPoint;
    private RobotController robotController;
    private StarterAssetsInputs playerInput;

    public override void OnNetworkSpawn()
    {        
        if (IsServer)
        {
            OnSpawn(RobotID.Value);
            Debug.Log($"[RefereeController] {RobotID.Value} Spawned");

            ResetAmmo();
        }

        // Debug.Log("Client:" + NetworkManager.Singleton.LocalClientId + "IsOwner?" + IsOwner);
        if (IsOwner) 
        {            
            if (FPVCamera != null & !robotTags.Contains(RobotTag.Building))
            {
                FPVCamera.TurnOnCamera();
                FPVCamera.SetRoleInfo(faction.Value, RobotID.Value);
            }  

            ShooterController[] Shooters = this.gameObject.GetComponentsInChildren<ShooterController>();
            int shooter1Mode = 0;
            int shooter2Mode = 0;
            foreach(var _shooter in Shooters)
            {
                if(_shooter != null)
                {
                    int id = _shooter.ID;
                    bool enabled = _shooter.Enabled.Value;

                    if (ShooterControllerList.ContainsKey(id))
                    {
                        Debug.LogError("[RefereeController] Duplicated shooter id!");
                        continue;
                    }

                    ShooterControllerList.Add(id, _shooter);

                    _shooter.gameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(this.gameObject.GetComponent<NetworkObject>().OwnerClientId);
                    _shooter.Enabled.Value = enabled;
                    _shooter.OnTrigger += TriggerHandler;

                    switch (id)
                    {
                        case 0:
                            shooter1Mode = _shooter.Mode.Value;
                            break;
                        case 2:
                            shooter2Mode = _shooter.Mode.Value;
                            break;
                        default:
                            break;
                    }
                }
            }

            OnPerformanceChange(RobotID.Value, ChassisMode.Value, shooter1Mode, shooter2Mode);

            robotController = this.gameObject.GetComponent<RobotController>();
            if (robotController != null) robotController.Enabled = true;

            playerInput = this.gameObject.GetComponent<StarterAssetsInputs>(); 
            if (playerInput != null) playerInput.enabled = true;     
        }
    }

    void Awake()
    {
        gameManager = GameObject.FindAnyObjectByType<GameManager>();
            
        FPVCamera = this.gameObject.GetComponentInChildren<FPVController>();

        Armors = this.gameObject.GetComponentsInChildren<ArmorController>();
    
        RFID = this.gameObject.GetComponentInChildren<RFIDController>();
    
        LightBar = this.gameObject.GetComponentInChildren<LightbarController>();

        UWB = this.gameObject.GetComponentInChildren<UWBController>();
    
        Wheels = this.gameObject.GetComponentsInChildren<WheelController>();
    }

    protected virtual void Start()
    {

    }

    void OnEnable()
    {
        foreach(ArmorController _armor in Armors)
        {
            if(_armor != null) 
            {
                _armor.OnHit += DamageHandler;
            }
        }

        foreach(var _shooter in ShooterControllerList.Values)
        {
            if(_shooter != null)
            {
                _shooter.OnTrigger += TriggerHandler;
            }
        }

        if (FPVCamera != null) FPVCamera.OnPerfChange += PerfChangeHandler;

        if (RFID != null) RFID.OnDetect += DetectHandler;        
    }

    void OnDisable()
    {
        // Debug.Log("Disable Armors: "+ Armors.Length);
        foreach(ArmorController _armor in Armors)
        {
            _armor.OnHit -= DamageHandler;
        }

        foreach(ShooterController _shooter in ShooterControllerList.Values)
        {
            if(_shooter != null)
            {
                _shooter.OnTrigger -= TriggerHandler;
            }
        }
        
        if (RFID != null) RFID.OnDetect -= DetectHandler;
    }

    protected virtual void Update()
    {
        if (robotController != null)
        {
            if (IsOwner) robotController.enabled = Enabled.Value;
        }

        // Sync Armor related Status
        foreach(ArmorController _armor in Armors)
        {
            if(_armor != null) 
            {
                _armor.Enabled = Enabled.Value;
                _armor.LightColor = faction.Value == Faction.Red ? 1 : 2;
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

        if (UWB != null)
        {
            // Debug.Log($"[RefereeController] {UWB.Position}, {UWB.Direction}");
            Position = UWB.Position;
            Direction = UWB.Direction;
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

            if (playerInput != null)
            {
                playerInput.cursorLocked = Enabled.Value;
            }
                
            if (FPVCamera != null)
            {
                int state;
                if (gameManager.isRunning.Value)
                {
                    state = 2;
                } else if (Ready.Value) {
                    state = 1;
                } else {
                    state = 0;
                }
                FPVCamera.SetReadyState(state);

                FPVCamera.SetRoleInfo(faction.Value, RobotID.Value);

                FPVCamera.SetHPLimit(HPLimit.Value);
                FPVCamera.SetHP(HP.Value);

                FPVCamera.SetGreyScale(!Enabled.Value);
                FPVCamera.SetReviveWindow(Reviving.Value);
                FPVCamera.SetReviveProgress(CurrentReviveProgress.Value, MaxReviveProgress.Value, (MaxReviveProgress.Value - CurrentReviveProgress.Value) / ReviveProgressPerSec.Value);

                FPVCamera.SetPurchaseRevive(PurchaseRevivePrice.Value >= gameManager.Coins.Value[(int)faction.Value], PurchaseRevivePrice.Value);

                FPVCamera.SetFreeRevive(CurrentReviveProgress.Value >= MaxReviveProgress.Value);

                bool has17mmShooter = false;
                bool has42mmShooter = false;

                foreach (var _shooter in ShooterControllerList.Values)
                {
                    if (!_shooter.Enabled.Value) continue;

                    switch (_shooter.Type.Value)
                    {
                        case 0:
                            has17mmShooter = true;
                            _shooter.SetAmmo(ConsumedAmmo0.Value, Ammo0.Value);
                            break;
                        case 1:
                            has42mmShooter = true;
                            _shooter.SetAmmo(ConsumedAmmo1.Value, Ammo1.Value);
                            break;
                        default:
                            break;
                    }
                }

                FPVCamera.SetAmmo0Item(has17mmShooter, InSupplyArea.Value, gameManager.Coins.Value[(int)faction.Value], Ammo0.Value, gameManager.Ammo0Supply.Value[(int)faction.Value]);
                FPVCamera.SetAmmo1Item(has42mmShooter, InSupplyArea.Value, gameManager.Coins.Value[(int)faction.Value], Ammo1.Value, gameManager.Ammo1Supply.Value[(int)faction.Value]);

                FPVCamera.SetPurchaseItem(robotTags.Contains(RobotTag.GroundUnit), gameManager.RemoteHPTimes.Value[(int)faction.Value], has17mmShooter, gameManager.RemoteAmmo0Times.Value[(int)faction.Value], has42mmShooter, gameManager.RemoteAmmo1Times.Value[(int)faction.Value]);

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
                FPVCamera.SetBuffer(Buffer.Value, BufferLimit.Value);
                FPVCamera.SetEnergy(Energy.Value, EnergyLimit.Value);
            }
        }

        if (IsServer)
        {
            // Debug.Log($"[RefereeController] Disengaged? {Disengaged.Value}, DisengagedTime: {DisengagedTime.Value}/{DisengagedTimeLimit.Value}");

            if (DisengagedTime.Value >= DisengagedTimeLimit.Value)
            {
                Disengaged.Value = true;
            } else {
                Disengaged.Value = false;
                DisengagedTime.Value += Time.deltaTime;
            }

            if (PowerLimit.Value > 0) TickPower();

            if (Reviving.Value) TickRevive();

            TickHeat();

            TickBuff();

            if (!Enabled.Value) return;

            if (EXPInfo != null & robotTags.Contains(RobotTag.GrowingUnit)) TickEXP();

            TickHealth();
        }

        // TickBuffServerRpc();
    }
    
    [Header("Player Status")]
    public NetworkVariable<bool> Enabled          = new NetworkVariable<bool>(true);
    public NetworkVariable<bool> Immutable         = new NetworkVariable<bool>(false);
    public NetworkVariable<int> Warning           = new NetworkVariable<int>(0);
    public NetworkVariable<int> OccupiedArea      = new NetworkVariable<int>(0);
    public NetworkVariable<bool> Ready         = new NetworkVariable<bool>(false);

    public NetworkVariable<bool> Disengaged      = new NetworkVariable<bool>(true);
    public NetworkVariable<float> DisengagedTime = new NetworkVariable<float>(0f);
    public NetworkVariable<float> DisengagedTimeLimit = new NetworkVariable<float>(6.0f);

    public Vector2 Position = Vector2.zero;
    public float Direction = 0f;

    public void GetReady()
    {
        if (gameManager.isRunning.Value) return;

        Ready.Value = !Ready.Value;

        Reset();

        if (Ready.Value)
        {
            // Back to spawn point & disable control
            gameObject.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            playerInput.enabled = false;
        } else {
            // enable control
            playerInput.enabled = true;
        }

        OnReady(RobotID.Value);
    }

    public void ToggleInput()
    {
        playerInput.enabled = true;
    }

    [Header("Power")]
    public NetworkVariable<float> PowerLimit        = new NetworkVariable<float>(0);
    public NetworkVariable<float> Power             = new NetworkVariable<float>(0);
    public NetworkVariable<float> BufferLimit       = new NetworkVariable<float>(0);
    public NetworkVariable<float> Buffer            = new NetworkVariable<float>(0);
    public NetworkVariable<float> EnergyLimit       = new NetworkVariable<float>(1900.0f);
    public NetworkVariable<float> Energy            = new NetworkVariable<float>(0);

    bool IsOverPower 
    {
        get { return (PowerLimit.Value < 0) ? false : (Power.Value > PowerLimit.Value); }
    }
    
    void TickPower()
    {
        float realPower = 0.0f;

        foreach (WheelController wheel in Wheels)
        {
            realPower += Mathf.Abs(wheel.GetPower());
        }

        float _deltaPower = (PowerLimit.Value - realPower) * Time.deltaTime;

        if (_deltaPower < 0)
        {
            if (Energy.Value > 0)
            {
                Energy.Value += _deltaPower;
                Power.Value = PowerLimit.Value;
            } else if (Buffer.Value > 0) {
                Buffer.Value += _deltaPower;
                Power.Value = realPower;
            } else {
                // Call overpower events for refreecontroller
                Power.Value = realPower;

                float overPowerK = (Power.Value - PowerLimit.Value) / PowerLimit.Value;
                float timeScale = Time.deltaTime / 0.1f;

                float _damage = 0f;
                if (overPowerK <= 0.1f & overPowerK > 0)
                {
                    _damage = 0.1f * HPLimit.Value * timeScale;
                } else if (overPowerK <= 0.2f & overPowerK > 0.1f)
                {
                    _damage = 0.2f * HPLimit.Value * timeScale;
                } else if (overPowerK > 0.2f)
                {
                    _damage = 0.4f * HPLimit.Value * timeScale;
                }

                RefereeDamage(_damage);
                Damage_OverPower.Value += _damage;
            }
        } else {
            if (Buffer.Value < BufferLimit.Value)
            {
                Buffer.Value += _deltaPower;
                Power.Value = PowerLimit.Value;
            } else if (Energy.Value < EnergyLimit.Value) {
                Energy.Value += _deltaPower;
                Power.Value = PowerLimit.Value;
            } else {
                Power.Value = realPower;
            }
        }
    }

    [Header("Health")]
    // PowerLimit: -1 - Unlimited
    public NetworkVariable<int> ShieldLimit       = new NetworkVariable<int>(0);
    public NetworkVariable<int> Shield            = new NetworkVariable<int>(0);
    public NetworkVariable<float> HPLimit           = new NetworkVariable<float>(500);
    public NetworkVariable<float> HP                = new NetworkVariable<float>(500);
    
    public NetworkVariable<float> Damage_Hit                = new NetworkVariable<float>(0);
    public NetworkVariable<float> Damage_17mm                = new NetworkVariable<float>(0);
    public NetworkVariable<float> Damage_42mm                = new NetworkVariable<float>(0);
    public NetworkVariable<float> Damage_Missle                = new NetworkVariable<float>(0);
    public NetworkVariable<float> Damage_OverPower                = new NetworkVariable<float>(0);
    public NetworkVariable<float> Damage_OverHeat                = new NetworkVariable<float>(0);
    public NetworkVariable<float> Damage_Warning = new NetworkVariable<float>(0);

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

    void DamageHandler(int damageType, float damage, int armorID, int attackerID)
    {
        if (!IsServer) return;

        if (attackerID != 0)
        {
            if (gameManager.RefereeControllerList[attackerID].faction.Value != faction.Value)
            {            
                if (AttackList.ContainsKey(attackerID)) 
                {
                    AttackList[attackerID].resetLastTime();
                } else {
                    AttackList.Add(attackerID, new AttackerInfo(attackerID));
                }
            }
        }

        float _hp = HP.Value;
        float _damage = damage;

        if (Enabled.Value && !Immutable.Value) {
            // Debug.Log("[GameManager - Damage] HP:"+RobotStatusList[robotID].HP);
            if (damageType != 3 & gameManager.RefereeControllerList.ContainsKey(attackerID)) 
            {
                int atkBuff = gameManager.RefereeControllerList[attackerID].ATKBuff.Value;
                if (atkBuff > 0) _damage = _damage * (1 + atkBuff / 100);
            }

            if (!robotTags.Contains(RobotTag.Building) & DEFBuff.Value > 0)
            {
                _damage = _damage * (1 - (DEFBuff.Value + DEFDeBuff.Value) / 100);
            }
            
            switch(damageType){
                case 0:
                    Damage_Hit.Value += _damage;
                    break;
                case 1:
                    Damage_17mm.Value += _damage;
                    break;
                case 2:
                    Damage_42mm.Value += _damage;
                    break;
                case 3:
                    Damage_Missle.Value += _damage;
                    break;
                default:
                    Debug.LogWarning("Unknown Damage Type" + damageType);
                    break;
            }

            if (_hp - _damage <= 0)
            {
                HP.Value = 0;
                ShooterEnabled.Value = false;
                Enabled.Value = false;
                OnDeath(attackerID, RobotID.Value);
            } else {
                HP.Value = (_hp - _damage);
            }
        }

        if (_damage > 0)
        {
            Disengaged.Value = false;
            DisengagedTime.Value = 0f;
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

    public int GetLastAttacker()
    {
        if (AttackList.Count <= 0) return 0;

        int attackerID = 0;

        foreach (var _attacker in AttackList.Values)
        {
            if (_attacker.lastTime > AttackList[attackerID].lastTime) attackerID = _attacker.ID;
        }

        return attackerID;
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

    void RefereeDamage(float damage)
    {
        if (Enabled.Value)
        {
            float _hp = HP.Value;
            
            Debug.Log($"[GameManager] raw damage: {damage}, damage {damage}");

            if (_hp - damage <= 0)
            {
                HP.Value = 0;
                Enabled.Value = false;
                ShooterEnabled.Value = false;
                OnDeath(RobotID.Value, RobotID.Value);
            } else {
                HP.Value = (_hp - damage);
            }

            if (damage > 0)
            {
                Disengaged.Value = false;
                DisengagedTime.Value = 0f;
            }
        }
    }

    public virtual void ShieldOff()
    {
        if (!IsServer) return;

        Shield.Value = 0;
    }

    void ResetDamage()
    {
        Damage_Hit.Value = 0f;
        Damage_17mm.Value = 0f; 
        Damage_42mm.Value = 0f;
        Damage_Missle.Value = 0f;
        Damage_OverPower.Value = 0f;
        Damage_OverHeat.Value = 0f;
        Damage_Warning.Value = 0f;
    }

    [Header("Revive")]
    public NetworkVariable<bool> Reviving          = new NetworkVariable<bool>(false);
    public NetworkVariable<int> MaxReviveProgress = new NetworkVariable<int>(10);
    public NetworkVariable<int> ReviveProgressPerSec = new NetworkVariable<int>(1);
    public NetworkVariable<float> CurrentReviveProgress = new NetworkVariable<float>(0);

    public NetworkVariable<int> PurchaseReviveTimes = new NetworkVariable<int>(2);
    public NetworkVariable<int> PurchaseRevivePrice = new NetworkVariable<int>(10);

    void TickRevive()
    {
        // Debug.Log($"[RefereeController] Revive Progress {CurrentReviveProgress.Value} / {MaxReviveProgress.Value}");

        if (CurrentReviveProgress.Value < MaxReviveProgress.Value) CurrentReviveProgress.Value += ReviveProgressPerSec.Value * Time.deltaTime;
    }

    public void Revive(int mode)
    {
        ReviveServerRPC(mode);
    }

    [ServerRpc]
    void ReviveServerRPC(int mode, ServerRpcParams serverRpcParams = default)
    {
        switch (mode)
        {
            case 0:
                if (CurrentReviveProgress.Value < MaxReviveProgress.Value) return;
                
                Reviving.Value = false;
                CurrentReviveProgress.Value = 0;
                HP.Value = HPLimit.Value * 10 / 100;
                Enabled.Value = true;

                OnRevived(RobotID.Value);
                break;
            case 1:
                // TODO: Pay to win
                Reviving.Value = false;
                CurrentReviveProgress.Value = 0;
                HP.Value = HPLimit.Value;
                ShooterEnabled.Value = true;
                Enabled.Value = true;
                break;
            default:
                break;
        }
    }

    # region Shooter related
    // Shooter Type: 0 - 17mm 1 - 42mm
    // Shooter Mode: 0 - None 1 - Boost 2 - CD 3 - Speed
    public NetworkVariable<bool> ShooterEnabled = new NetworkVariable<bool>(true);

    // Ammo: 0 - 17mm 1 - 42 mm
    // Ammo0 - 17mm
    public NetworkVariable<int> ConsumedAmmo0     = new NetworkVariable<int>(0);
    public NetworkVariable<int> Ammo0             = new NetworkVariable<int>(0);
    public NetworkVariable<int> RealAmmo0         = new NetworkVariable<int>(350);
    public NetworkVariable<int> RealAmmo0Limit         = new NetworkVariable<int>(350);

    // Ammo1 - 42mm
    public NetworkVariable<int> ConsumedAmmo1     = new NetworkVariable<int>(0);
    public NetworkVariable<int> Ammo1             = new NetworkVariable<int>(0);
    public NetworkVariable<int> RealAmmo1         = new NetworkVariable<int>(100);
    public NetworkVariable<int> RealAmmo1Limit         = new NetworkVariable<int>(100);

    void TriggerHandler(int ID, Vector3 Position, Vector3 Velocity)
    {
        // TODO: handle trigger events

        // First judge whether the unit is disabled or not
        if (!Enabled.Value || !ShooterEnabled.Value || !ShooterControllerList.ContainsKey(ID)) return;

        ShooterController shooter = ShooterControllerList[ID];

        switch (shooter.Type.Value)
        {
            case 0:
                if (Ammo0.Value <= 0 || RealAmmo0.Value <= 0) return;
                
                shooter.Heat.Value += 10;
                
                ConsumedAmmo0.Value ++;
                Ammo0.Value --;
                RealAmmo0.Value --;
                break;
            case 1:
                if (Ammo1.Value <= 0 || RealAmmo1.Value <= 0) return;

                shooter.Heat.Value += 100;

                ConsumedAmmo1.Value ++;
                Ammo1.Value --;
                RealAmmo1.Value --;
                break;
            default:
                Debug.LogWarning("[GameManager] Unknown shooter ID");
                break;
        }

        Disengaged.Value = false;
        DisengagedTime.Value = 0f;

        OnShoot(ID, shooter.Type.Value, RobotID.Value, Position, Velocity);
    }

    void TickHeat()
    {
        int CDFactor = 1;
        if (CDBuff.Value > 0) CDFactor = CDBuff.Value;

        foreach (var _shooter in ShooterControllerList.Values)
        {
            if (!_shooter.Enabled.Value || _shooter.Heat.Value <= 0) continue;

            if (_shooter.Heat.Value > _shooter.HeatLimit.Value & _shooter.Heat.Value <= 2*_shooter.HeatLimit.Value)
            {
                RefereeDamage((_shooter.Heat.Value - _shooter.HeatLimit.Value) / 250 / 10 * HPLimit.Value);
            } else if (_shooter.Heat.Value >= 2*_shooter.HeatLimit.Value) {
                RefereeDamage((_shooter.Heat.Value - 2*_shooter.HeatLimit.Value) / 250 * HPLimit.Value);
                _shooter.Heat.Value = 2*_shooter.HeatLimit.Value;
            }
            
            // Shooter 0 CD
            if (_shooter.Heat.Value >= _shooter.CD.Value * CDFactor * Time.deltaTime) 
            {
                _shooter.Heat.Value -= _shooter.CD.Value * CDFactor * Time.deltaTime;
            } else {
                _shooter.Heat.Value = 0;
            }
        }
    }

    void ResetAmmo()
    {
        ConsumedAmmo0.Value = 0;
        Ammo0.Value = 0;
        RealAmmo0.Value = RealAmmo0Limit.Value;
        ConsumedAmmo1.Value = 0;
        Ammo1.Value = 0;
        RealAmmo1.Value = RealAmmo1Limit.Value;
    }

    #endregion

    #region Purchase

    public NetworkVariable<bool> InSupplyArea = new NetworkVariable<bool>(false);

    // 0 - HP, 1 - 17mm, 2 - 42mm
    public void RemotePurchase(PurchaseType type, int amount = 1)
    {
        // Debug.Log($"[RefereeController] type {type}, amount {amount}");
        RemotePurchaseServerRpc(type, amount);
    }

    [ServerRpc]
    void RemotePurchaseServerRpc(PurchaseType type, int amount, ServerRpcParams serverRpcParams = default)
    {
        if (!Enabled.Value) return;
        if (!Disengaged.Value) return;

        switch(type)
        {
            case PurchaseType.Remote_HP:
                if(gameManager.RemoteHPTimes.Value[(int)faction.Value] == 0) return;
                break;
            case PurchaseType.Remote_Ammo0:
                if(gameManager.RemoteAmmo0Times.Value[(int)faction.Value] == 0) return;
                break;
            case PurchaseType.Remote_Ammo1:
                if(gameManager.RemoteAmmo1Times.Value[(int)faction.Value] == 0) return;
                break;
            default:
                Debug.LogError("[RefereeController] Unknown Purchase Type"); 
                return;
        }

        OnPurchase(RobotID.Value, type, amount);
    }

    public void Purchase(PurchaseType type, int amount)
    {
        PurchaseServerRpc(type, amount);
    }

    [ServerRpc]
    void PurchaseServerRpc(PurchaseType type, int amount, ServerRpcParams serverRpcParams = default)
    {
        if (!InSupplyArea.Value) return;

        OnPurchase(RobotID.Value, type, amount);
    }

    #endregion

    #region Buff Related
    
    [Header("Buff Related")]
    
    public NetworkVariable<int> ATKBuff         = new NetworkVariable<int>(0);
    public NetworkVariable<int> DEFBuff         = new NetworkVariable<int>(0);
    public NetworkVariable<int> DEFDeBuff         = new NetworkVariable<int>(0);
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

        // Debug.Log($"[RefereeController] activeBuff Counts {activeBuffs.Count}");

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

                if (_buff.DEFDeBuff > newBuffStat.DEFDeBuff)
                {
                    newBuffStat.DEFDeBuff = _buff.DEFDeBuff;
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

                if (_buff.InSupplyArea > newBuffStat.InSupplyArea)
                {
                    newBuffStat.InSupplyArea = _buff.InSupplyArea;
                }

                _buffInfo.lastTime += Time.deltaTime;
                if (_buffInfo.lastTime * 1000 > _buff.buffDuration) overtimeBuff.Add(_buff);
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
        DEFDeBuff.Value = newBuffStat.DEFDeBuff;
        ATKBuff.Value = newBuffStat.ATKBuff;
        CDBuff.Value = newBuffStat.CDBuff;
        InSupplyArea.Value = (newBuffStat.InSupplyArea > 0);
    }

    void DetectHandler(int areaID)
    {
        if (!Enabled.Value) return;

        OnOccupy(areaID, RobotID.Value);
    }

    #endregion

    #region EXP related

    public NetworkVariable<int> Level             = new NetworkVariable<int>(0);
    public NetworkVariable<int> EXP               = new NetworkVariable<int>(0);
    public NetworkVariable<int> EXPToNextLevel    = new NetworkVariable<int>(0);
    public NetworkVariable<int> EXPValue          = new NetworkVariable<int>(0);
    public NetworkVariable<float> TimeToNextEXP = new NetworkVariable<float>(0.0f);

    public NetworkVariable<int> ChassisMode = new NetworkVariable<int>(0);
    public RobotPerformanceSO ChassisPerformance;

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

        // Level up
        if (EXP.Value >= EXPInfo.expToNextLevel[Level.Value] && EXPInfo.expToNextLevel[Level.Value] >= 0)
        {
            // Don't zero the current EXP, just minus the EXP needed to next level
            EXP.Value -= EXPInfo.expToNextLevel[Level.Value];
            Level.Value += 1;
            EXPToNextLevel.Value = EXPInfo.expToNextLevel[Level.Value];

            EXPValue.Value = EXPInfo.expValue[Level.Value];

            if (ChassisPerformance != null)
            {
                float _recoverHP = ChassisPerformance.maxHealth[Level.Value] - HPLimit.Value;
                HPLimit.Value = ChassisPerformance.maxHealth[Level.Value];
                HP.Value += _recoverHP;
                PowerLimit.Value = ChassisPerformance.maxPower[Level.Value];
            }

            foreach (var _shooter in ShooterControllerList.Values)
            {
                _shooter.Level.Value = Level.Value;
                if (_shooter.GimbalPerformance == null) continue;

                _shooter.HeatLimit.Value = _shooter.GimbalPerformance.maxHeat[Level.Value];
                _shooter.CD.Value = _shooter.GimbalPerformance.coolDown[Level.Value];
                _shooter.SpeedLimit.Value = _shooter.GimbalPerformance.shootSpeed[Level.Value];
            }
        }
    }

    void PerfChangeHandler(int chassisMode, int shooter1Mode, int shooter2Mode)
    {
        OnPerformanceChange(RobotID.Value, chassisMode, shooter1Mode, shooter2Mode);
    }

    #endregion

    #region Mine
    
    public Stack<OreController> OreList = new Stack<OreController>();
    public List<Transform> OreStorePoints = new List<Transform>();
    public List<GripperController> GripperPoints = new List<GripperController>();
    public NetworkVariable<int[]> ExchangeSpeed = new NetworkVariable<int[]>();

    public void AddOre(OreController ore)
    {
        if (OreList.Count >= OreStorePoints.Count) return;

        OreList.Push(ore);

        UpdateOre();
    }

    public OreController RemoveOre()
    {
        OreController ore = OreList.Pop();

        UpdateOre();

        return ore;
    }

    void UpdateOre()
    {
        // Debug.Log($"[RefereeController] OreList {OreList.Count}");

        int i = 0;
        foreach (var ore in OreList)
        {
            // Debug.Log($"[RefereeController] Ore {ore}");

            ore.gameObject.transform.SetPositionAndRotation(OreStorePoints[i].position, OreStorePoints[i].rotation);
              
            i ++;
        }
    }

    #endregion

    public virtual void Reset()
    {
        Level.Value = 0;
        EXP.Value = 0;
        EXPToNextLevel.Value = EXPInfo.expToNextLevel[Level.Value];
        EXPValue.Value = EXPInfo.expValue[Level.Value];
        TimeToNextEXP.Value = 0;

        Reviving.Value = false;
        MaxReviveProgress.Value = 10;
        ReviveProgressPerSec.Value = defaultBuff.ReviveProgressPerSec;
        CurrentReviveProgress.Value = 0;

        HPLimit.Value = ChassisPerformance.maxHealth[Level.Value];
        HP.Value = HPLimit.Value;
        Shield.Value = ShieldLimit.Value;
        PowerLimit.Value = ChassisPerformance.maxPower[Level.Value];

        ResetDamage();
        
        ATKBuff.Value = defaultBuff.ATKBuff;
        DEFBuff.Value = defaultBuff.DEFBuff;
        CDBuff.Value = defaultBuff.CDBuff;
        HealBuff.Value = defaultBuff.HealBuff;

        ResetAmmo();

        foreach(var _shooter in ShooterControllerList.Values)
        {
            _shooter.Reset();
        }

        ShooterEnabled.Value = true;

        while(OreList.Count > 0)
        {
            Destroy(OreList.Pop());
        }

        Enabled.Value = true;
        Warning.Value = 0;
        OccupiedArea.Value = 0;
        // Immutable value reset in override method according to game logic
    }
}
