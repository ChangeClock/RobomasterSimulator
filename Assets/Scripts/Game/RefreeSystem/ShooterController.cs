using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ShooterController : NetworkBehaviour 
{
    // Only upload the shooter postion, velocity and ShooterID, all the other details like whether this shootaction is valid will be judged by refereecontroller
    // This is called trigger action is because the player only pulls the trigger, whether there will be a bullet is determined by other factors...
    public delegate void TriggerAction(int shooterID, Vector3 userPosition, Vector3 shootVelocity);
    public event TriggerAction OnTrigger;

    public GameObject ShootPoint;

    [SerializeField] public NetworkVariable<bool> Enabled  = new NetworkVariable<bool>(true);
    // public bool Enabled = false;
    public int ID = 0;

    // // Shooter Type: 0 - 17mm 1 - 42mm
    public NetworkVariable<int> Type      = new NetworkVariable<int>(0);


    // Shooter 17mm Mode: 0 - None 1 - Boost 2 - CD 3 - Speed
    // Shooter 42mm Mode: 0 - None 1 - Boost 2 - Speed
    public NetworkVariable<int> Mode      = new NetworkVariable<int>(0);

    public RobotPerformanceSO GimbalPerformance;
    public NetworkVariable<int> Level             = new NetworkVariable<int>(0);

    public NetworkVariable<float> HeatLimit        = new NetworkVariable<float>(0f);
    public NetworkVariable<float> Heat             = new NetworkVariable<float>(0f);
    public NetworkVariable<int> CD               = new NetworkVariable<int>(0);
    public NetworkVariable<int> SpeedLimit       = new NetworkVariable<int>(0);

    [SerializeField] private GameObject UI;
    [SerializeField] private TextMeshProUGUI Speed;
    [SerializeField] private TextMeshProUGUI Ammo;
    [SerializeField] private BarController HeatBar;

    [SerializeField] private GameObject BlurBackground;
    [SerializeField] private GameObject OverHeat;

    void Start()
    {
        // UI.SetActive(IsOwner);
    }

    void Update()
    {
        // Update light effects on shooter components according to head and heatlimit

        HeatBar.SetValue(Heat.Value);
        HeatBar.SetMaxValue(HeatLimit.Value);
        
        if (Heat.Value < HeatLimit.Value / 2)
        {
            HeatBar.SetColor(Color.white);
        } else if (Heat.Value >= HeatLimit.Value / 2 && Heat.Value < HeatLimit.Value * 3/4) {
            HeatBar.SetColor(Color.yellow);
        } else {
            HeatBar.SetColor(Color.red);
        }

        BlurBackground.SetActive(Heat.Value > HeatLimit.Value);
        OverHeat.SetActive(Heat.Value > HeatLimit.Value);
    }

    public void PullTrigger()
    {
        if (!Enabled.Value) return;

        // Debug.Log("[ShooterController] PullTrigger");
        OnTrigger(ID, ShootPoint.transform.position, ShootPoint.transform.right * SpeedLimit.Value * 10);
    }

    public void SetAmmo(int ammo, int ammoLimit)
    {
        Ammo.text = ammo.ToString() + "/" + ammoLimit.ToString();
    }

    public void SetHeatMode(int mode)
    {
        // Round heat bar
    }

    public void ToggleUI()
    {
        UI.SetActive(!UI.activeSelf);
    }

    public void Reset()
    {
        Level.Value = 0;

        Heat.Value = 0;
        HeatLimit.Value = GimbalPerformance.maxHeat[Level.Value];
        CD.Value = GimbalPerformance.coolDown[Level.Value];
        SpeedLimit.Value = GimbalPerformance.shootSpeed[Level.Value];
    }
}