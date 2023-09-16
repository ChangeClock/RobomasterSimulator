using System;
using UnityEngine;
using TMPro;

public class MatchInfoController : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private TMP_Text Time;
    [SerializeField] private TMP_Text RedBaseHP;
    [SerializeField] private TMP_Text RedBaseShield;
    [SerializeField] private HealthBarController RedBaseHPBar;
    [SerializeField] private TMP_Text BlueBaseHP;
    [SerializeField] private TMP_Text BlueBaseShield;
    [SerializeField] private HealthBarController BlueBaseHPBar;
    [SerializeField] private TMP_Text RedSentryHP;
    [SerializeField] private TMP_Text RedSentryAmmo;
    [SerializeField] private HealthBarController RedSentryHPBar;
    [SerializeField] private TMP_Text BlueSentryHP;
    [SerializeField] private TMP_Text BlueSentryAmmo;
    [SerializeField] private HealthBarController BlueSentryHPBar;
    [SerializeField] private TMP_Text RedOutpostHP;
    [SerializeField] private HealthBarController RedOutpostHPBar;
    [SerializeField] private TMP_Text BlueOutpostHP;
    [SerializeField] private HealthBarController BlueOutpostHPBar;
    
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
        RedBaseHPBar.SetHealth(gameManager.RedBase.HP.Value);

        // Blue Base
        
        BlueBaseHP.text = gameManager.BlueBase.HP.Value.ToString();
        BlueBaseShield.text = gameManager.BlueBase.Shield.Value.ToString();
        BlueBaseHPBar.SetHealth(gameManager.BlueBase.HP.Value);

        // Red Sentry

        if (gameManager.RedSentry != null)
        {
            RedSentryHP.text = gameManager.RedSentry.HP.Value.ToString();
            RedSentryAmmo.text = gameManager.RedSentry.Ammo0.Value.ToString();
            RedSentryHPBar.SetHealth(gameManager.RedSentry.HP.Value);
        }
        
        // Blue Sentry

        if (gameManager.BlueSentry != null)
        {
            BlueSentryHP.text = gameManager.BlueSentry.HP.Value.ToString();
            BlueSentryAmmo.text = gameManager.BlueSentry.Ammo0.Value.ToString();
            BlueSentryHPBar.SetHealth(gameManager.BlueSentry.HP.Value);
        }

        // Red Outpost

        RedOutpostHP.text = gameManager.RedOutpost.HP.Value.ToString();
        RedOutpostHPBar.SetHealth(gameManager.RedOutpost.HP.Value);

        // Blue Outpost
        
        BlueOutpostHP.text = gameManager.BlueOutpost.HP.Value.ToString();
        BlueOutpostHPBar.SetHealth(gameManager.BlueOutpost.HP.Value);

        // Red Coin

        // Blue Coin
    }
}