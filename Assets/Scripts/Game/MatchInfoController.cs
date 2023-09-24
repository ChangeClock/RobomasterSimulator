using System;
using UnityEngine;
using TMPro;

public class MatchInfoController : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private TMP_Text Time;
    [SerializeField] private TMP_Text RedBaseHP;
    [SerializeField] private TMP_Text RedBaseShield;
    [SerializeField] private BarController RedBaseHPBar;
    [SerializeField] private TMP_Text BlueBaseHP;
    [SerializeField] private TMP_Text BlueBaseShield;
    [SerializeField] private BarController BlueBaseHPBar;
    [SerializeField] private TMP_Text RedSentryHP;
    [SerializeField] private TMP_Text RedSentryAmmo;
    [SerializeField] private BarController RedSentryHPBar;
    [SerializeField] private TMP_Text BlueSentryHP;
    [SerializeField] private TMP_Text BlueSentryAmmo;
    [SerializeField] private BarController BlueSentryHPBar;
    [SerializeField] private TMP_Text RedOutpostHP;
    [SerializeField] private BarController RedOutpostHPBar;
    [SerializeField] private TMP_Text BlueOutpostHP;
    [SerializeField] private BarController BlueOutpostHPBar;

    [SerializeField] private UnitInfoBar[] UnitInfoBars;
    
    [SerializeField] private TMP_Text RedCoin;
    [SerializeField] private TMP_Text BlueCoin;

    void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        // Time

        TimeSpan timeSpan = TimeSpan.FromSeconds(gameManager.TimeLeft.Value);
        Time.text = timeSpan.Minutes.ToString("0") + " : " + timeSpan.Seconds.ToString("00");

        // Red Base

        RedBaseHP.text = gameManager.RedBase.HP.Value.ToString();
        RedBaseShield.text = gameManager.RedBase.Shield.Value.ToString();
        RedBaseHPBar.SetValue(gameManager.RedBase.HP.Value);

        // Blue Base
        
        BlueBaseHP.text = gameManager.BlueBase.HP.Value.ToString();
        BlueBaseShield.text = gameManager.BlueBase.Shield.Value.ToString();
        BlueBaseHPBar.SetValue(gameManager.BlueBase.HP.Value);

        // Red Sentry

        if (gameManager.RedSentry != null)
        {
            RedSentryHP.text = gameManager.RedSentry.HP.Value.ToString();
            RedSentryAmmo.text = gameManager.RedSentry.Ammo0.Value.ToString();
            RedSentryHPBar.SetValue(gameManager.RedSentry.HP.Value);
        }
        
        // Blue Sentry

        if (gameManager.BlueSentry != null)
        {
            BlueSentryHP.text = gameManager.BlueSentry.HP.Value.ToString();
            BlueSentryAmmo.text = gameManager.BlueSentry.Ammo0.Value.ToString();
            BlueSentryHPBar.SetValue(gameManager.BlueSentry.HP.Value);
        }

        // Red Outpost

        RedOutpostHP.text = gameManager.RedOutpost.HP.Value.ToString();
        RedOutpostHPBar.SetValue(gameManager.RedOutpost.HP.Value);

        // Blue Outpost
        
        BlueOutpostHP.text = gameManager.BlueOutpost.HP.Value.ToString();
        BlueOutpostHPBar.SetValue(gameManager.BlueOutpost.HP.Value);

        // Red Coin

        // Blue Coin

        
        // Sync Observer UI
        // if (ObserverUI != null)
        // {
        //     ObserverUI.SetID(robotClass.Value == RobotClass.Sentry ? "AI" : RobotID.ToString());
        //     ObserverUI.SetHealth(HP.Value, HPLimit.Value);
        //     ObserverUI.SetLevelInfo(true, Level.Value);
        //     ObserverUI.SetBuff(ATKBuff.Value>0, CDBuff.Value>0, DEFBuff.Value>0, HealBuff.Value>0);
        // }
    }
}