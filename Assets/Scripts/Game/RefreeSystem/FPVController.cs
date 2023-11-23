using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Cinemachine;

public class FPVController : MonoBehaviour
{
    private RefereeController referee;

    [SerializeField] private CinemachineVirtualCamera FPVCamera;
    [SerializeField] private AudioListener FPVAudioListener;
    [SerializeField] private GameObject PlayerUI;

    void Awake() 
    {
        referee = gameObject.GetComponentInParent<RefereeController>();
    
        SettingMenuAction.performed += context => ToggleSettingMenu();    
        
        if (referee.robotClass.Value == RobotClass.Engineer)
        {
            ExchangeMenu.SetActive(true);
            RemotePurchaseMenu.SetActive(false);
            RemotePurchaseAction.performed += context => ToggleExchangeMenu();
        } else {
            ExchangeMenu.SetActive(false);
            RemotePurchaseMenu.SetActive(true);
            RemotePurchaseAction.performed += context => ToggleRemotePurchaseMenu();
        }

        Ammo0PurchaseAction.performed += context => ToggleAmmo0PurchaseMenu();
        Ammo1PurchaseAction.performed += context => ToggleAmmo1PurchaseMenu();

        int i = 0;
        foreach (var item in LevelItems)
        {
            item.gameObject.GetComponent<Button>().onClick.AddListener(() => ChangeLevelHandler(i));
            i ++;
        }
    }

    void Start()
    {
        if (referee != null)
        {
            PurchaseRevive.onClick.AddListener(() => referee.Revive(1));
            FreeRevive.onClick.AddListener(() => referee.Revive(0));
            SetRobotSetting.onClick.AddListener(() => ToggleRobotSettingMenu(true));
            SaveSettingsButton.onClick.AddListener(() => ToggleRobotSettingMenu(false));
            GetReady.onClick.AddListener(() => referee.GetReady());

            RemotePurchaseHPAction.performed += context => {referee.RemotePurchase(PurchaseType.Remote_HP);};
            RemotePurchaseAmmo0Action.performed += context => {referee.RemotePurchase(PurchaseType.Remote_Ammo0);};
            RemotePurchaseAmmo1Action.performed += context => {referee.RemotePurchase(PurchaseType.Remote_Ammo1);};
        
            Ammo0_50.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo0, 50));
            Ammo0_100.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo0, 100));
            Ammo0_150.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo0, 150));
            Ammo0_200.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo0, 200));
            Ammo0_250.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo0, 250));
            Ammo0_300.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo0, 300));

            Ammo1_5.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo1, 5));
            Ammo1_10.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo1, 10));
            Ammo1_15.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo1, 15));
            Ammo1_20.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo1, 20));
            Ammo1_25.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo1, 25));
            Ammo1_30.onClick.AddListener(() => referee.Purchase(PurchaseType.Ammo1, 30));
        
            ConfirmExchangeChoice.onClick.AddListener(() => ChooseExchangeLevel());
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

        Ammo0PurchaseAction.Enable();
        Ammo1PurchaseAction.Enable();

        GameManager.OnBuffActived += BuffActivatePopup;
    }

    void OnDisable()
    {
        SettingMenuAction.Disable();
        RemotePurchaseAction.Disable();
        RemotePurchaseHPAction.Disable();
        RemotePurchaseAmmo0Action.Disable();
        RemotePurchaseAmmo1Action.Disable();

        Ammo0PurchaseAction.Disable();
        Ammo1PurchaseAction.Disable();

        GameManager.OnBuffActived -= BuffActivatePopup;
    }

    // Update is called once per frame
    void Update()
    {
        if (referee.robotClass.Value != RobotClass.Engineer)
        {
            if (referee.Disengaged.Value)
            {
                HintTextUnavailable.SetActive(false);
                HintTextOpen.SetActive(!RemotePurchaseList.activeSelf);
                HintTextClose.SetActive(RemotePurchaseList.activeSelf);
            } else {
                HintTextUnavailable.SetActive(true);
                RemotePurchaseList.SetActive(false);
                HintTextOpen.SetActive(false);
                HintTextClose.SetActive(false);
            }
        } else {
            HintTextOpenExchange.SetActive(!LevelChoiceMenu.activeSelf);
            HintTextCloseExchange.SetActive(LevelChoiceMenu.activeSelf);
        }

        if (ReviveWindow.activeSelf || SettingMenu.activeSelf || Ammo0PurchaseMenu.activeSelf || Ammo1PurchaseMenu.activeSelf || LevelChoiceMenu.activeSelf)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        } else {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }

        SetRobotSetting.interactable = !referee.Ready.Value;
    }

    public void TurnOnCamera()
    {
        FPVCamera.enabled = true;
        FPVAudioListener.enabled = true;
        PlayerUI.SetActive(true);
    }

    public void TurnOffCamera()
    {
        FPVCamera.enabled = false;
        FPVAudioListener.enabled = false;
        PlayerUI.SetActive(false);
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
    [SerializeField] private RawImage Avatar;
    [SerializeField] private Texture HeroAvatar;
    [SerializeField] private Texture EngineerAvatar;
    [SerializeField] private Texture InfantryAvatar;
    [SerializeField] private Texture SentryAvatar;
    [SerializeField] private Texture AeroAvatar;

    public void SetRoleInfo(Faction belong, int robotID, RobotClass robotclass)
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

        switch(robotclass)
        {
            case RobotClass.Hero:
                Avatar.texture = HeroAvatar;
                break;
            case RobotClass.Engineer:
                Avatar.texture = EngineerAvatar;
                break;
            case RobotClass.Infantry:
                Avatar.texture = InfantryAvatar;
                break;
            case RobotClass.Sentry:
                Avatar.texture = SentryAvatar;
                break;
            case RobotClass.Aero:
                Avatar.texture = AeroAvatar;
                break;
            default:
                break;
        }
    }

    #endregion

    #region HPStatus

    [SerializeField] private BarController HPBar;
    [SerializeField] private TextMeshProUGUI HP;
    [SerializeField] private TextMeshProUGUI HPLimit;

    public void SetHP(float hp, float hplimit)
    {
        HPLimit.text = hplimit.ToString("N0");
        HPBar.SetMaxValue(hplimit);
        HP.text = hp.ToString("N0");
        HPBar.SetValue(hp);
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
        TimeToRevive.text = time.ToString("N0") + "s";
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

    public void SetExpInfo(float EXP, float EXPToNextLevel)
    {
        EXPBar.SetMaxValue(EXPToNextLevel);
        EXPBar.SetValue(EXP);
    }

    #endregion

    #region MarkStatus

    [SerializeField] private GameObject MarkedStatus;

    public void SetMarkedStatus(bool enable)
    {
        MarkedStatus.SetActive(enable);
    }

    #endregion

    #region BuffStatus

    [SerializeField] private GameObject HealBuff;
    [SerializeField] private GameObject DEFBuff;
    [SerializeField] private GameObject DEFDownBuff;
    [SerializeField] private GameObject CDBuff;
    [SerializeField] private GameObject ATKBuff;
    [SerializeField] private TextMeshProUGUI HealBuffValue;
    [SerializeField] private TextMeshProUGUI DEFBuffValue;
    [SerializeField] private TextMeshProUGUI DEFDownBuffValue;
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

    public void SetDEFDeBuff(bool enable, int buffValue = 25)
    {
        DEFDownBuff.SetActive(enable);
        DEFDownBuffValue.text = "-" + buffValue.ToString() + "%";
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
    [SerializeField] private Button SetRobotSetting;
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

    #region Purchase

    [SerializeField] private InputAction Ammo0PurchaseAction;

    [SerializeField] private GameObject Ammo0PurchaseMenu;
    [SerializeField] private TextMeshProUGUI Ammo0Status;
    [SerializeField] private GameObject NoShooterPopup0;
    [SerializeField] private GameObject NotInSupplyArea0;
    [SerializeField] private Button Ammo0_50;
    [SerializeField] private Button Ammo0_100;
    [SerializeField] private Button Ammo0_150;
    [SerializeField] private Button Ammo0_200;
    [SerializeField] private Button Ammo0_250;
    [SerializeField] private Button Ammo0_300;

    [SerializeField] private InputAction Ammo1PurchaseAction;

    [SerializeField] private GameObject Ammo1PurchaseMenu;
    [SerializeField] private TextMeshProUGUI Ammo1Status;
    [SerializeField] private GameObject NoShooterPopup1;
    [SerializeField] private GameObject NotInSupplyArea1;
    [SerializeField] private Button Ammo1_5;
    [SerializeField] private Button Ammo1_10;
    [SerializeField] private Button Ammo1_15;
    [SerializeField] private Button Ammo1_20;
    [SerializeField] private Button Ammo1_25;
    [SerializeField] private Button Ammo1_30;

    void ToggleAmmo0PurchaseMenu()
    {
        Ammo0PurchaseMenu.SetActive(!Ammo0PurchaseMenu.activeSelf);
    }

    public void SetAmmo0Item(bool hasShooter, bool inSupplyArea, int coin, int currentAmmo, int maxAmmo)
    {
        int availableAmmo = (coin > maxAmmo) ? maxAmmo : coin;

        Ammo0Status.text = currentAmmo.ToString() + "/" + availableAmmo.ToString();

        NoShooterPopup0.SetActive(!hasShooter);
        NotInSupplyArea0.SetActive(!inSupplyArea);

        if (50 <= availableAmmo)
        {
            Ammo0_50.interactable = true;
        } else {
            Ammo0_50.interactable = false;
        }

        if (100 <= availableAmmo)
        {
            Ammo0_100.interactable = true;
        } else {
            Ammo0_100.interactable = false;
        }

        if (150 <= availableAmmo)
        {
            Ammo0_150.interactable = true;
        } else {
            Ammo0_150.interactable = false;
        }

        if (200 <= availableAmmo)
        {
            Ammo0_200.interactable = true;
        } else {
            Ammo0_200.interactable = false;
        }

        if (250 <= availableAmmo)
        {
            Ammo0_250.interactable = true;
        } else {
            Ammo0_250.interactable = false;
        }

        if (300 <= availableAmmo)
        {
            Ammo0_300.interactable = true;
        } else {
            Ammo0_300.interactable = false;
        }
    }

    void ToggleAmmo1PurchaseMenu()
    {
        Ammo1PurchaseMenu.SetActive(!Ammo1PurchaseMenu.activeSelf);
    }

    public void SetAmmo1Item(bool hasShooter, bool inSupplyArea, int coin, int currentAmmo, int maxAmmo)
    {
        int availableAmmo = ((coin / 15) > maxAmmo) ? maxAmmo : (coin / 15);

        Ammo1Status.text = currentAmmo.ToString() + "/" + availableAmmo.ToString();

        NoShooterPopup1.SetActive(!hasShooter);
        NotInSupplyArea1.SetActive(!inSupplyArea);

        if (!hasShooter || !inSupplyArea)
        {
            Ammo1_5.interactable = false;
            Ammo1_10.interactable = false;
            Ammo1_15.interactable = false;
            Ammo1_20.interactable = false;
            Ammo1_25.interactable = false;
            Ammo1_30.interactable = false;

            return;
        }

        if (5 <= availableAmmo)
        {
            Ammo1_5.interactable = true;
        } else {
            Ammo1_5.interactable = false;
        }

        if (10 <= availableAmmo)
        {
            Ammo1_10.interactable = true;
        } else {
            Ammo1_10.interactable = false;
        }

        if (15 <= availableAmmo)
        {
            Ammo1_15.interactable = true;
        } else {
            Ammo1_15.interactable = false;
        }

        if (20 <= availableAmmo)
        {
            Ammo1_20.interactable = true;
        } else {
            Ammo1_20.interactable = false;
        }

        if (25 <= availableAmmo)
        {
            Ammo1_25.interactable = true;
        } else {
            Ammo1_25.interactable = false;
        }

        if (30 <= availableAmmo)
        {
            Ammo1_30.interactable = true;
        } else {
            Ammo1_30.interactable = false;
        }
    }

    #endregion

    [SerializeField] private InputAction RemotePurchaseAction;

    #region RemotePurchase

    [SerializeField] private GameObject RemotePurchaseMenu;

    [SerializeField] private GameObject RemotePurchaseList;
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

        RemotePurchaseList.SetActive(!RemotePurchaseList.activeSelf);
    }

    public void SetPurchaseItem(bool hp, int hpTimes, bool bullet_17, int bullet_17Times, bool bullet_42, int bullet_42Times)
    {
        if (hp)
        {
            PurchaseItemHP.alpha = 1f;
            if (PurchaseItemHPPoint1 != null) PurchaseItemHPPoint1.color = (hpTimes >=1) ? ActiveColor : InactiveColor;
            if (PurchaseItemHPPoint2 != null) PurchaseItemHPPoint2.color = (hpTimes >=2) ? ActiveColor : InactiveColor;
            RemotePurchaseHPAction.Enable();
        } else {
            PurchaseItemHP.alpha = 0.4f;
            RemotePurchaseHPAction.Disable();
        }
        
        if (bullet_17)
        {
            PurchaseItem17mm.alpha = 1f;
            if (PurchaseItem17mmPoint1 != null) PurchaseItem17mmPoint1.color = (bullet_17Times >=1) ? ActiveColor : InactiveColor;
            if (PurchaseItem17mmPoint2 != null) PurchaseItem17mmPoint2.color = (bullet_17Times >=2) ? ActiveColor : InactiveColor;
            RemotePurchaseAmmo0Action.Enable();
        } else {
            PurchaseItem17mm.alpha = 0.4f;
            RemotePurchaseAmmo0Action.Disable();
        }
        
        if (bullet_42)
        {
            PurchaseItem42mm.alpha = 1f;
            if (PurchaseItem42mmPoint1 != null) PurchaseItem42mmPoint1.color = (bullet_42Times >=1) ? ActiveColor : InactiveColor;
            if (PurchaseItem42mmPoint2 != null) PurchaseItem42mmPoint2.color = (bullet_42Times >=2) ? ActiveColor : InactiveColor;
            RemotePurchaseAmmo1Action.Enable();
        } else {
            PurchaseItem42mm.alpha = 0.4f;
            RemotePurchaseAmmo1Action.Disable();
        }
    }

    #endregion

    #region Exchange & Mine

    [SerializeField] private GameObject ExchangeMenu;

    [SerializeField] private GameObject LevelChoiceMenu;
    [SerializeField] private GameObject HintTextOpenExchange;
    [SerializeField] private GameObject HintTextCloseExchange;

    [SerializeField] private List<ExchangeLevelItemController> LevelItems = new List<ExchangeLevelItemController>();
    [SerializeField] private Button ConfirmExchangeChoice;

    [SerializeField] private GameObject Level;

    [SerializeField] private BarController ExchangeProgress;
    [SerializeField] private BarController MineProgress;

    void ToggleExchangeMenu()
    {
        LevelChoiceMenu.SetActive(!LevelChoiceMenu.activeSelf);
    }

    public void SetExchangeMenu(ExchangePriceSO priceInfo, int leastLevel)
    {
        int i = 0;
        foreach (var item in LevelItems)
        {
            item.SetAvailability(leastLevel <= i);
            item.SetContent("Level" + (i + 1).ToString(), priceInfo.silverPrice[i].ToString(), priceInfo.goldPrice[i].ToString());
            i ++;
        }
    }

    public void SetExchangeStatus(int level, int status, float progress, float maxprogress, float loss)
    {
        Level.SetActive(status >= 1);
        if (status == 1)
        {
            Level.GetComponentInChildren<TMP_Text>().text = "Level" + (level+1).ToString() + " Loss: " + (loss * 100).ToString("F1") + "%";
            ExchangeProgress.SetMaxValue(maxprogress);
            ExchangeProgress.SetValue(progress);
        } else if (status == 2)
        {
            Level.GetComponentInChildren<TMP_Text>().text = "Resetting";
        }
    }

    public void SetMiningStatus(OreType type, float progress, float maxprogress)
    {
        MineProgress.SetColor((type == OreType.Silver) ? Color.white : Color.yellow);
        MineProgress.SetMaxValue(maxprogress);
        MineProgress.SetValue(progress);
    }

    void ChangeLevelHandler(int level)
    {
        int i = 0;
        foreach (var item in LevelItems)
        {
            item.Toggle(level == i);
            i ++;
        }
    }

    void ChooseExchangeLevel()
    {
        int i = 0;
        foreach (var item in LevelItems)
        {
            if (item.Checked)
            {
                referee.ChooseExchangeLevel(true, i);
                ToggleExchangeMenu();
                return;
            }
            i ++;
        }
    }

    void CancelExchange()
    {
        referee.ChooseExchangeLevel(false);
    }

    #endregion

    #region Buff

    [SerializeField] private GameObject BuffIndicator;
    [SerializeField] private GameObject MissleIndicator;
    [SerializeField] private GameObject PopupText;

    public void BuffActivatePopup(Faction faction, BuffType type, int totalScore, BuffEffectSO effect)
    {
        StartCoroutine(BuffActivatePopupCoroutine(type));

        // String text = $"{faction.ToString()} activate {type.ToString()} buff {type == BuffType.Big ? "with score: " totalScore.ToString() + "\br " : ""}";
        String text = "";
        if (type == BuffType.Big)
        {
            text = $"{faction.ToString()} activated {type.ToString()} buff with score: {totalScore.ToString()}";
        } else {
            text = $"{faction.ToString()} activated {type.ToString()} buff";
        }
        
        if (effect != null)
        {
            text += $"\n Gained ";
            if (effect.ATKBuff > 0)
            {
                text += $"{effect.ATKBuff.ToString()}% ATK ";
            }
            if (effect.DEFBuff > 0)
            {
                text += $"{effect.DEFBuff.ToString()}% DEF";
            }
        }
        
        StartCoroutine(PopupTextCoroutine(text));
    }

    public IEnumerator BuffActivatePopupCoroutine(BuffType type)
    {
        BuffIndicator.GetComponent<RawImage>().color = (referee.faction.Value == Faction.Red) ? Color.red : Color.blue;
        BuffIndicator.SetActive(true);
        yield return new WaitForSeconds(3f);
        BuffIndicator.SetActive(false);
    }

    public IEnumerator PopupTextCoroutine(String text)
    {
        PopupText.GetComponentInChildren<TextMeshProUGUI>().text = text;
        PopupText.SetActive(true);
        yield return new WaitForSeconds(3f);
        PopupText.SetActive(false);
    }

    #endregion

    #region Shoot

    #endregion

    // Start is called before the first frame update

    [Header("Robot Settings")]
    [SerializeField] private CinemachineVirtualCamera ThirdPersonCamera;
    [SerializeField] private GameObject RobotSettingMenu;

    [SerializeField] private GameObject ShooterSettings;
    [SerializeField] private TMP_InputField RealAmmo0Input;
    [SerializeField] private TMP_InputField RealAmmo1Input;
    [SerializeField] private TMP_InputField MinEnclosingCircleRaiusInput;

    [SerializeField] private GameObject BuffActivateSettings;
    [SerializeField] private TMP_InputField SmallBuffHitRateInput;
    [SerializeField] private TMP_InputField BigBuffHitRateInput;
    [SerializeField] private TMP_InputField BigBuffHighestScoreInput;

    [SerializeField] private GameObject ExchangeSettings;
    [SerializeField] private TMP_InputField MineSilverSpeedInput;
    [SerializeField] private TMP_InputField MineGoldSpeedInput;
    [SerializeField] private List<TMP_InputField> ExchangeSpeedInput = new List<TMP_InputField>();

    [SerializeField] private Button SaveSettingsButton;

    void InitSettingMenu()
    {
        if (referee.ShooterControllerList.Count > 0)
        {
            ShooterSettings.SetActive(true);
            BuffActivateSettings.SetActive(true);
            RealAmmo0Input.text = referee.RealAmmo0Limit.Value.ToString();
            RealAmmo1Input.text = referee.RealAmmo1Limit.Value.ToString();
            MinEnclosingCircleRaiusInput.text = referee.MinEnclosingCircle_Raius.Value.ToString();
        } else {
            ShooterSettings.SetActive(false);
            BuffActivateSettings.SetActive(false);
        }
    }

    void ToggleRobotSettingMenu(bool enable)
    {
        RobotSettingMenu.SetActive(enable);

        if (!RobotSettingMenu.activeSelf) 
        {
            ApplyRobotSettings();
        }
    }

    void ToggleThirdPersonCamera(bool enable)
    {
        FPVCamera.enabled = !enable;
        ThirdPersonCamera.enabled = enable;
    }

    void ApplyRobotSettings()
    {
        referee.RealAmmo0Limit.Value = int.Parse(RealAmmo0Input.text);
        referee.RealAmmo1Limit.Value = int.Parse(RealAmmo1Input.text);
        referee.MinEnclosingCircle_Raius.Value = float.Parse(MinEnclosingCircleRaiusInput.text);
        referee.SmallBuffHitRate.Value = float.Parse(SmallBuffHitRateInput.text);
        referee.BigBuffHitRate.Value = float.Parse(BigBuffHitRateInput.text);
        referee.BigBuffHighestScore.Value = int.Parse(BigBuffHighestScoreInput.text);
        referee.MineSilverSpeed.Value = int.Parse(MineSilverSpeedInput.text);
        referee.MineGoldSpeed.Value = int.Parse(MineGoldSpeedInput.text);
        for (int i = 0; i < ExchangeSpeedInput.Count; i++)
        {
            referee.ExchangeSpeed[i] = int.Parse(ExchangeSpeedInput[i].text);
        }
    }
}
