using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class FPVController : MonoBehaviour
{
    private RefereeController referee;

    [SerializeField] private Camera FPVCamera;
    [SerializeField] private GameObject PlayerUI;

    void Start()
    {
        referee = gameObject.GetComponentInParent<RefereeController>();
    
        if (referee != null)
        {
            PurchaseRevive.onClick.AddListener(() => referee.Revive(1));
            FreeRevive.onClick.AddListener(() => referee.Revive(0));
        }
    }

    void OnEnable()
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

    // // Vision and Warning status
    // [SerializeField] private GameObject OverLay;
    // [SerializeField] private GameObject Info;
    // [SerializeField] private RawImage InfoBackground;
    // [SerializeField] private TextMeshProUGUI InfoContent;

    // // 1: 黄牌
    // public void SetWarning(int mode, Faction faction, int id, )
    // {
    //     OverLay.SetActive(enable);
    //     if ()
    // }

    // public void SetVision(bool enable)
    // {
    //     OverLay.SetActive(enable);
    // }

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

    #region ReviveStatus 

    [SerializeField] private GameObject GrayScale;
    [SerializeField] private GameObject ReviveWindow;
    [SerializeField] private TextMeshProUGUI TimeToRevive;
    [SerializeField] private Slider ReviveProgress;
    [SerializeField] private TextMeshProUGUI RevivePrice;
    [SerializeField] private GameObject PayRevivePrice;
    [SerializeField] private GameObject InsufficientFund;
    [SerializeField] private Button PurchaseRevive;
    [SerializeField] private Button FreeRevive;

    public void SetGreyScale(bool enable)
    {
        GrayScale.SetActive(enable);
    }

    public void SetReviveWindow(bool enable)
    {
        ReviveWindow.SetActive(enable);
    }

    public void SetReviveProgress(float current, float max, float time)
    {
        ReviveProgress.maxValue = max;
        ReviveProgress.value = current;
        TimeToRevive.text = time.ToString("N0");
    }

    public void SetPurchaseRevive(bool enable, int price)
    {
        RevivePrice.text = price.ToString();

        PurchaseRevive.interactable = enable;
        PayRevivePrice.SetActive(enable);
        InsufficientFund.SetActive(!enable);
    }

    public void SetFreeRevive(bool enable)
    {
        FreeRevive.interactable = enable;
    }

    #endregion

    #region LevelStatus

    [SerializeField] private GameObject[] LevelIcons;
    [SerializeField] private BarController EXPBar;

    public void SetLevelInfo(int level)
    {
        for (int i = 0; i < LevelIcons.Length; i++)
        {
            if (level == i)
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

    [SerializeField] private GameObject BlurBackground;
    [SerializeField] private GameObject OverHeat;

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

    public void SetHeat0(float heat0, float limit0)
    {
        Shooter0Heat.SetValue(heat0);
        Shooter0Heat.SetMaxValue(limit0);
        if (heat0 < limit0 / 2)
        {
            Shooter0Heat.SetColor(Color.white);
        } else if (heat0 >= limit0 / 2 && heat0 < limit0 * 3/4) {
            Shooter0Heat.SetColor(Color.yellow);
        } else {
            Shooter0Heat.SetColor(Color.red);
        }

        BlurBackground.SetActive(heat0 > limit0);
        OverHeat.SetActive(heat0 > limit0);
    }

    public void SetHeat1(float heat1, float limit1)
    {
        Shooter1Heat.SetValue(heat1);
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
