using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class FPVController : MonoBehaviour
{
    [SerializeField] private Camera FPVCamera;
    [SerializeField] private GameObject PlayerUI;

    // This is used to disable vision on Aero side
    [SerializeField] public bool Enabled = true;
    [SerializeField] public int Warning = 0;

    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TurnOnCamera()
    {
        FPVCamera.enabled = true;
        PlayerUI.SetActive(true);
    }

    #region RoleStatus

    [SerializeField] private TextMeshProUGUI ID;

    public void SetRoleInfo(Faction belong, int robotID)
    {
        switch(belong)
        {
            case Faction.Red:
                HPBar.SetColor(Color.red);
                ID.text = robotID.ToString();
                break;
            case Faction.Blue:
                HPBar.SetColor(Color.blue);
                ID.text = (robotID - 20).ToString();
                break;
            default:
                Debug.LogError("[FPVController] Unknown faction");
                break;
        }
    }

    #endregion

    #region HPStatus

    [SerializeField] private BarController HPBar;
    [SerializeField] private TextMeshProUGUI HP;
    [SerializeField] private TextMeshProUGUI HPLimit;

    public void SetHP(float hp)
    {
        HP.text = hp.ToString("N0");
        HPBar.SetValue(hp);
    }

    public void SetHPLimit(float hplimit)
    {
        HPLimit.text = hplimit.ToString("N0");
        HPBar.SetMaxValue(hplimit);
    }

    #endregion

    #region LevelStatus

    [SerializeField] private GameObject[] LevelIcons;
    [SerializeField] private BarController EXPBar;

    public void SetLevelInfo(int Level)
    {
        for (int i = 0; i < LevelIcons.Length; i++)
        {
            if (Level == i)
            {
                LevelIcons[i].SetActive(true);
            } else {
                LevelIcons[i].SetActive(false);
            }
        }
    }

    public void SetExpInfo(int EXP, int EXPToNextLevel)
    {
        EXPBar.SetMaxValue(EXPToNextLevel);
        EXPBar.SetValue(EXP);
    }

    #endregion

    #region BuffStatus

    [SerializeField] private GameObject HealBuff;
    [SerializeField] private GameObject DEFBuff;
    [SerializeField] private GameObject CDBuff;
    [SerializeField] private GameObject ATKBuff;
    [SerializeField] private TextMeshProUGUI HealBuffValue;
    [SerializeField] private TextMeshProUGUI DEFBuffValue;
    [SerializeField] private TextMeshProUGUI CDBuffValue;
    [SerializeField] private TextMeshProUGUI ATKBuffValue;

    public void SetHealBuff(bool enable, int buffValue = 25)
    {
        HealBuff.SetActive(enable);
        HealBuffValue.text = buffValue.ToString() + "%";
    }

    public void SetDEFBuff(bool enable, int buffValue = 25)
    {
        DEFBuff.SetActive(enable);
        DEFBuffValue.text = buffValue.ToString() + "%";
    }

    public void SetCDBuff(bool enable, int buffValue = 3)
    {
        CDBuff.SetActive(enable);
        CDBuffValue.text = "x" + buffValue.ToString();
    }

    public void SetATKBuff(bool enable, int buffValue = 50)
    {
        ATKBuff.SetActive(enable);
        ATKBuffValue.text = buffValue.ToString() + "%";
    }

    #endregion

    [SerializeField] private GameObject Shooter0Info;
    [SerializeField] private TextMeshProUGUI Shooter0Speed;
    [SerializeField] private TextMeshProUGUI Shooter0Ammo;
    [SerializeField] private BarController Shooter0Heat;

    [SerializeField] private GameObject Shooter1Info;
    [SerializeField] private TextMeshProUGUI Shooter1Speed;
    [SerializeField] private TextMeshProUGUI Shooter1Ammo;
    [SerializeField] private BarController Shooter1Heat;

    public void SetShooterInfo(bool Shooter0Enable, bool Shooter1Enable)
    {
        Shooter0Info.SetActive(Shooter0Enable);
        Shooter1Info.SetActive(Shooter1Enable);
    }

    public void SetAmmo(int shooter0Ammo, int shooter0AmmoLimit, int shooter1Ammo, int shooter1AmmoLimit)
    {
        Shooter0Ammo.text = shooter0Ammo.ToString() + "/" + shooter0AmmoLimit.ToString();
        Shooter1Ammo.text = shooter1Ammo.ToString() + "/" + shooter1AmmoLimit.ToString();
    }

    public void SetHeat(float heat0, float heat1)
    {
        Shooter0Heat.SetValue(heat0);
        Shooter1Heat.SetValue(heat1);
    }

    public void SetHeatLimit(float limit0, float limit1)
    {
        Shooter0Heat.SetMaxValue(limit0);
        Shooter1Heat.SetMaxValue(limit1);
    }

    [SerializeField] private BarController BufferBar;
    [SerializeField] private TextMeshProUGUI Power;
    
    public void SetMaxBuffer(float bufferLimit)
    {
        BufferBar.SetMaxValue(bufferLimit);
    }

    public void SetBuffer(float var)
    {
        BufferBar.SetValue(var);
    }

    // Mode: 0-Normal 1-Overpower
    public void SetPower(float var, int mode = 0)
    {
        Power.text = var.ToString("F1") + "w";

        switch (mode)
        {
            case 1:
                Power.color = Color.red;
                break;
            default:
                Power.color = Color.white;
                break;
        }
    }
 
    // public override void OnStartLocalPlayer()
    // {
    //     if (IsLocalPlayer)
    //     {
    //         FPVCamera.enabled = true;
    //     }
    // }

    // Start is called before the first frame update
}
