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

    [SerializeField] private BarController HPBar;
    [SerializeField] private TextMeshProUGUI HP;
    [SerializeField] private TextMeshProUGUI HPLimit;

    [SerializeField] private GameObject[] LevelIcons;
    [SerializeField] private BarController EXPBar;

    [SerializeField] private GameObject Shooter0Info;
    [SerializeField] private TextMeshProUGUI Shooter0Speed;
    [SerializeField] private TextMeshProUGUI Shooter0Ammo;
    [SerializeField] private BarController Shooter0Heat;

    [SerializeField] private GameObject Shooter1Info;
    [SerializeField] private TextMeshProUGUI Shooter1Speed;
    [SerializeField] private TextMeshProUGUI Shooter1Ammo;
    [SerializeField] private BarController Shooter1Heat;

    [SerializeField] private BarController BufferBar;
    [SerializeField] private TextMeshProUGUI Power;
 
    // public override void OnStartLocalPlayer()
    // {
    //     if (IsLocalPlayer)
    //     {
    //         FPVCamera.enabled = true;
    //     }
    // }

    // Start is called before the first frame update
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

    public void SetHP(int hp)
    {
        HP.text = hp.ToString();
        HPBar.SetValue(hp);
    }

    public void SetHPLimit(int hplimit)
    {
        HPLimit.text = hplimit.ToString();
        HPBar.SetMaxValue(hplimit);
    }

    public void SetColor(Color color)
    {
        HPBar.SetColor(color);
    }

    public void SetLevelInfo(int Level)
    {
        for (int i = 0; i< 4; i++)
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
}
