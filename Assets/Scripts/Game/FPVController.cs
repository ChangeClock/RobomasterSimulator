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

    void Start()
    {
        referee = gameObject.GetComponentInParent<RefereeController>();
    
        if (referee != null)
        {
            PurchaseRevive.onClick.AddListener(() => referee.Revive(1));
            FreeRevive.onClick.AddListener(() => referee.Revive(0));
            GetReady.onClick.AddListener(() => referee.GetReady());
        }

        ChassisMode.onValueChanged.AddListener(delegate {SetPerformance(ChassisMode);});
        Shooter1Mode.onValueChanged.AddListener(delegate {SetPerformance(Shooter1Mode);});
        Shooter2Mode.onValueChanged.AddListener(delegate {SetPerformance(Shooter2Mode);});

    }

    void Awake() 
    {
        SettingMenuAction.performed += context => ToggleSettingMenu();    
    }

    void OnEnable()
    {
        SettingMenuAction.Enable();
    }

    void OnDisable()
    {
        SettingMenuAction.Disable();
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

    // Start is called before the first frame update
}
