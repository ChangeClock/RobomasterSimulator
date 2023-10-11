using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class FPVController : MonoBehaviour
{
    private RefereeController referee;

    [SerializeField] private Camera FPVCamera;
    [SerializeField] private GameObject PlayerUI;

    void Awake() 
    {
        referee = gameObject.GetComponentInParent<RefereeController>();
    
        SettingMenuAction.performed += context => ToggleSettingMenu();    
        
        if (referee.robotClass.Value == RobotClass.Engineer)
        {
            RemotePurchaseAction.performed += context => ToggleExchangeMenu();
        } else {
            RemotePurchaseAction.performed += context => ToggleRemotePurchaseMenu();
        }

    }

    void Start()
    {
        if (referee != null)
        {
            PurchaseRevive.onClick.AddListener(() => referee.Revive(1));
            FreeRevive.onClick.AddListener(() => referee.Revive(0));
            GetReady.onClick.AddListener(() => referee.GetReady());

            RemotePurchaseHPAction.performed += context => {referee.RemotePurchase(PurchaseType.Remote_HP);};
            RemotePurchaseAmmo0Action.performed += context => {referee.RemotePurchase(PurchaseType.Remote_Ammo0);};
            RemotePurchaseAmmo1Action.performed += context => {referee.RemotePurchase(PurchaseType.Remote_Ammo1);};
        }

        ChassisMode.onValueChanged.AddListener(delegate {SetPerformance(ChassisMode);});
        Shooter1Mode.onValueChanged.AddListener(delegate {SetPerformance(Shooter1Mode);});
        Shooter2Mode.onValueChanged.AddListener(delegate {SetPerformance(Shooter2Mode);});
    }

    void OnEnable()
    {
        SettingMenuAction.Enable();
        RemotePurchaseAction.Enable();
        RemotePurchaseHPAction.Enable();
        RemotePurchaseAmmo0Action.Enable();
        RemotePurchaseAmmo1Action.Enable();
    }

    void OnDisable()
    {
        SettingMenuAction.Disable();
        RemotePurchaseAction.Disable();
        RemotePurchaseHPAction.Disable();
        RemotePurchaseAmmo0Action.Disable();
        RemotePurchaseAmmo1Action.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (referee.robotClass.Value != RobotClass.Engineer)
        {
            if (referee.Disengaged.Value)
            {
                HintTextUnavailable.SetActive(false);
                HintTextOpen.SetActive(!RemotePurchaseMenu.activeSelf);
                HintTextClose.SetActive(RemotePurchaseMenu.activeSelf);
            } else {
                HintTextUnavailable.SetActive(true);
                RemotePurchaseMenu.SetActive(false);
                HintTextOpen.SetActive(false);
                HintTextClose.SetActive(false);
            }
        }

        if (ReviveWindow.activeSelf || SettingMenu.activeSelf)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        } else {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }
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
                Role.text = "R" + robotID.ToString();
                break;
            case Faction.Blue:
                HPBar.SetColor(Color.blue);
                ID.text = (robotID - 20).ToString();
                Role.text = "B" + robotID.ToString();
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

    [SerializeField] private BarController EnergyBar;
    [SerializeField] private BarController BufferBar;
    [SerializeField] private TextMeshProUGUI Power;
    
    public void SetEnergy(float var, float max)
    {
        EnergyBar.SetMaxValue(max);
        EnergyBar.SetValue(var);
    }

    public void SetBuffer(float var, float max)
    {
        BufferBar.SetMaxValue(max);
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

    #region Setting Menu

    [SerializeField] private InputAction SettingMenuAction;
    [SerializeField] private GameObject SettingMenu;
    [SerializeField] private Button GetReady;
    [SerializeField] private TMP_Text State;
    [SerializeField] private TMP_Text Role;
    [SerializeField] private GameObject ChassisPerformance;
    [SerializeField] private TMP_Dropdown ChassisMode;
    [SerializeField] private List<TMP_Dropdown.OptionData> InfantryChassisOptions;
    [SerializeField] private List<TMP_Dropdown.OptionData> HeroChassisOptions;
    [SerializeField] private GameObject Shooter1Performance;
    [SerializeField] private TMP_Dropdown Shooter1Mode;
    [SerializeField] private GameObject Shooter2Performance;
    [SerializeField] private TMP_Dropdown Shooter2Mode;
    [SerializeField] private List<TMP_Dropdown.OptionData> Options_17mm;
    [SerializeField] private List<TMP_Dropdown.OptionData> Options_42mm;

    public delegate void ChangePerfRequest(int chassisMode, int shooter1Mode, int shooter2Mode);
    public event ChangePerfRequest OnPerfChange;

    void ToggleSettingMenu()
    {
        SettingMenu.SetActive(!SettingMenu.activeSelf);

        Shooter1Performance.SetActive(referee.ShooterControllerList.ContainsKey(0));

        Shooter2Performance.SetActive(referee.ShooterControllerList.ContainsKey(1));

        switch (referee.robotClass.Value)
        {
            case RobotClass.Hero:
                ChassisPerformance.SetActive(true);
                ChassisMode.options = HeroChassisOptions;
                ChassisMode.value = referee.ChassisMode.Value;
                break;
            case RobotClass.Infantry:
                ChassisPerformance.SetActive(true);
                ChassisMode.options = InfantryChassisOptions;
                ChassisMode.value = referee.ChassisMode.Value;
                break;
            default:
                ChassisPerformance.SetActive(false);
                break;    
        }

        if (referee.ShooterControllerList.ContainsKey(0))
        {
            Shooter1Performance.SetActive(true);

            if (referee.ShooterControllerList[0].Enabled.Value)
            {
                Shooter1Mode.interactable = true;

                switch(referee.ShooterControllerList[0].Type.Value)
                {
                    case 0:
                        Shooter1Mode.options = Options_17mm;
                        break;
                    case 1:
                        Shooter1Mode.options = Options_42mm;
                        break;
                    default:
                        Shooter1Mode.interactable = false;
                        break;
                }

                Shooter1Mode.value = referee.ShooterControllerList[0].Mode.Value;
            } else {
                Shooter1Mode.interactable = false;
            }
        }

        if (referee.ShooterControllerList.ContainsKey(1))
        {
            Shooter2Performance.SetActive(true);

            if (referee.ShooterControllerList[1].Enabled.Value)
            {
                Shooter2Mode.interactable = true;

                switch(referee.ShooterControllerList[1].Type.Value)
                {
                    case 0:
                        Shooter2Mode.options = Options_17mm;
                        break;
                    case 1:
                        Shooter2Mode.options = Options_42mm;
                        break;
                    default:
                        Shooter2Mode.interactable = false;
                        break;
                }

                Shooter2Mode.value = referee.ShooterControllerList[1].Mode.Value;
            } else {
                Shooter2Mode.interactable = false;
            }
        }
    }   

    void SetPerformance(TMP_Dropdown change)
    {
        Debug.Log($"[FPVController] chassis: {ChassisMode.value}, shooter1: {Shooter1Mode.value}, shooter2: {Shooter2Mode.value}");
        OnPerfChange(ChassisMode.value, Shooter1Mode.value, Shooter2Mode.value);
    }

    public void SetReadyState(int mode)
    {
        switch (mode)
        {
            case 0:
                State.text = "Ready";
                State.color = Color.green;
                GetReady.interactable = true;
                break;
            case 1:
                State.text = "Waiting";
                State.color = Color.yellow;
                GetReady.interactable = true;
                break;
            case 2:
                State.text = "Running";
                State.color = Color.red;
                GetReady.interactable = false;
                break;
            default:
                break;
        }
    }

    #endregion

    #region RemotePurchase

    [SerializeField] private InputAction RemotePurchaseAction;

    [SerializeField] private GameObject RemotePurchaseMenu;
    [SerializeField] private GameObject HintTextOpen;
    [SerializeField] private GameObject HintTextClose;
    [SerializeField] private GameObject HintTextUnavailable;

    [SerializeField] private Color InactiveColor = Color.gray;
    [SerializeField] private Color ActiveColor = new Color(146, 255, 252, 255);

    [SerializeField] private InputAction RemotePurchaseHPAction;
    [SerializeField] private CanvasGroup PurchaseItemHP;
    [SerializeField] private Image PurchaseItemHPPoint1;
    [SerializeField] private Image PurchaseItemHPPoint2;

    [SerializeField] private InputAction RemotePurchaseAmmo0Action;
    [SerializeField] private CanvasGroup PurchaseItem17mm;
    [SerializeField] private Image PurchaseItem17mmPoint1;
    [SerializeField] private Image PurchaseItem17mmPoint2;

    [SerializeField] private InputAction RemotePurchaseAmmo1Action;
    [SerializeField] private CanvasGroup PurchaseItem42mm;
    [SerializeField] private Image PurchaseItem42mmPoint1;
    [SerializeField] private Image PurchaseItem42mmPoint2;

    void ToggleRemotePurchaseMenu()
    {
        if (!referee.Disengaged.Value) return;

        RemotePurchaseMenu.SetActive(!RemotePurchaseMenu.activeSelf);
    }

    public void SetPurchaseItem(bool hp, int hpTimes, bool bullet_17, int bullet_17Times, bool bullet_42, int bullet_42Times)
    {
        if (hp)
        {
            PurchaseItemHP.alpha = 1f;
            if (PurchaseItemHPPoint1 != null) PurchaseItemHPPoint1.color = (hpTimes >=1) ? ActiveColor : InactiveColor;
            if (PurchaseItemHPPoint2 != null) PurchaseItemHPPoint2.color = (hpTimes >=2) ? ActiveColor : InactiveColor;
        } else {
            PurchaseItemHP.alpha = 0.4f;
        }
        
        if (bullet_17)
        {
            PurchaseItem17mm.alpha = 1f;
            if (PurchaseItem17mmPoint1 != null) PurchaseItem17mmPoint1.color = (bullet_17Times >=1) ? ActiveColor : InactiveColor;
            if (PurchaseItem17mmPoint2 != null) PurchaseItem17mmPoint2.color = (bullet_17Times >=2) ? ActiveColor : InactiveColor;
        } else {
            PurchaseItem17mm.alpha = 0.4f;
        }
        
        if (bullet_42)
        {
            PurchaseItem42mm.alpha = 1f;
            if (PurchaseItem42mmPoint1 != null) PurchaseItem42mmPoint1.color = (bullet_42Times >=1) ? ActiveColor : InactiveColor;
            if (PurchaseItem42mmPoint2 != null) PurchaseItem42mmPoint2.color = (bullet_42Times >=2) ? ActiveColor : InactiveColor;
        } else {
            PurchaseItem42mm.alpha = 0.4f;
        }
    }

    #endregion

    #region Exchange

    private GameObject ExchangeMenu;

    void ToggleExchangeMenu()
    {

    }

    #endregion

    // Start is called before the first frame update
}
