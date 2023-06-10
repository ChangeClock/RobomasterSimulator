using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RefereeController : NetworkBehaviour
{
    public delegate void DamageAction(int damageType, int armorID, int robotID);
    public event DamageAction OnDamage;

    public DataTransmission.RobotStatus Status = new DataTransmission.RobotStatus();

    [Header("Referee")]
    private ArmorController[] Armors;
    public int RobotID;

    void Start()
    {
        Debug.Log("[RefreeController] HP: " + Status.GetHP());
    }

    void OnEnable()
    {
        Armors = this.gameObject.GetComponentsInChildren<ArmorController>();
        Debug.Log("Enable Armors: "+ Armors.Length);
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
    }

    void DamageHandler(int damageType, int armorID)
    {
        OnDamage(damageType, armorID, RobotID);
    }
}
