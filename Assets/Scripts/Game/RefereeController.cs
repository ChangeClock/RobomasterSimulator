using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class RefereeController : NetworkBehaviour
{
    public delegate void DamageAction(int damageType, int armorID, int robotID);
    public event DamageAction OnDamage;

    public DataTransmission.RobotStatus Status = new DataTransmission.RobotStatus();

    [SerializeField] private TextMeshProUGUI ObserverUI;

    [Header("Status")]
    [SerializeField] private NetworkVariable<int> HPLimit = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> HP = new NetworkVariable<int>(500, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Shooter0Enabled = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Shooter1Enabled = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Heat1Limit = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Heat1 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Heat2Limit = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Heat2 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Disabled = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Immutable = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> Level = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> EXP = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Referee")]
    private ArmorController[] Armors;
    public int RobotID;

    void Start()
    {
        Debug.Log("[RefreeController] HP: " + Status.GetHP());
        Debug.Log("Enable Armors: "+ Armors.Length);
    }

    void OnEnable()
    {
        Armors = this.gameObject.GetComponentsInChildren<ArmorController>();
        foreach(ArmorController _armor in Armors)
        {
            _armor.OnHit += DamageHandler;
            _armor.lightColor = RobotID < 20 ? 1 : 2;
        }
    }

    void OnDisable()
    {
        Debug.Log("Disable Armors: "+ Armors.Length);
        foreach(ArmorController _armor in Armors)
        {
            _armor.OnHit -= DamageHandler;
        }
    }

    void Update()
    {
        // Sync Armor related Status
        foreach(ArmorController _armor in Armors)
        {
            if(_armor != null) {
                _armor.disabled = Status.GetDisabled();
            }
        }

        // Sync Observer UI
        ObserverUI.text = "Player: " + (OwnerClientId) + "\n" + "HP: " + (HP.Value.ToString());
    }

    void DamageHandler(int damageType, int armorID)
    {
        OnDamage(damageType, armorID, RobotID);
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
