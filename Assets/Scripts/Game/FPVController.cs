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

    [SerializeField] private HealthBarController HPBar;
    [SerializeField] private TextMeshProUGUI HP;
    [SerializeField] private TextMeshProUGUI HPLimit;

    [SerializeField] private GameObject[] LevelIcons;
    [SerializeField] private HealthBarController EXPBar;

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
        HPBar.SetHealth(hp);
    }

    public void SetHPLimit(int hplimit)
    {
        HPLimit.text = hplimit.ToString();
        HPBar.SetMaxHealth(hplimit);
    }

    public void SetColor(Color color)
    {
        HPBar.SetColor(color);
    }

    public void SetLevelInfo(int Level, int EXP, int EXPToNextLevel)
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

        EXPBar.SetMaxHealth(EXPToNextLevel);
        EXPBar.SetHealth(EXP);
    }
}
