using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class MatchInfoController : NetworkBehaviour
{
    [SerializeField] private GameManager gameManager;

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

    [SerializeField] private GameObject[] RedUnitInfo;
    [SerializeField] private GameObject[] BlueUnitInfo;
    
    [SerializeField] private TMP_Text RedCoin;
    [SerializeField] private TMP_Text BlueCoin;
    [SerializeField] private TMP_Text RedCoinTotal;
    [SerializeField] private TMP_Text BlueCoinTotal;

    [SerializeField] private MapController MiniMap;
    [SerializeField] private Faction userBelong = Faction.Neu;
    [SerializeField] private int userID = 0;

    void Start()
    {
        if (gameManager == null) gameManager = GameObject.FindFirstObjectByType<GameManager>();
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
        // Debug.Log($"[MatchInfoController] Coins: {gameManager.Coins.Value.Length}");
        RedCoin.text = gameManager.Coins[(int)Faction.Red].ToString();
        RedCoinTotal.text = gameManager.CoinsTotal[(int)Faction.Red].ToString();

        // Blue Coin
        BlueCoin.text = gameManager.Coins[(int)Faction.Blue].ToString();
        BlueCoinTotal.text = gameManager.CoinsTotal[(int)Faction.Blue].ToString();
        
        for (int i = 1; i <= 7; i ++)
        {
            RedUnitInfo[i-1].SetActive(gameManager.RefereeControllerList.ContainsKey(i));

            if (RedUnitInfo[i-1].activeSelf)
            {
                RedUnitInfo[i-1].GetComponent<UnitInfoController>().SetID(i);
                RedUnitInfo[i-1].GetComponent<UnitInfoController>().SetHP(gameManager.RefereeControllerList[i].HP.Value, gameManager.RefereeControllerList[i].HPLimit.Value);
                RedUnitInfo[i-1].GetComponent<UnitInfoController>().SetLevel(gameManager.RefereeControllerList[i].Level.Value);
                RedUnitInfo[i-1].GetComponent<UnitInfoController>().SetReviveTime(gameManager.RefereeControllerList[i].Reviving.Value, (gameManager.RefereeControllerList[i].MaxReviveProgress.Value - gameManager.RefereeControllerList[i].CurrentReviveProgress.Value) / gameManager.RefereeControllerList[i].ReviveProgressPerSec.Value);
            }
        }

        for (int i = 21; i <= 27; i ++)
        {
            BlueUnitInfo[i-21].SetActive(gameManager.RefereeControllerList.ContainsKey(i));

            if (BlueUnitInfo[i-21].activeSelf)
            {
                BlueUnitInfo[i-21].GetComponent<UnitInfoController>().SetID(i - 20);
                BlueUnitInfo[i-21].GetComponent<UnitInfoController>().SetHP(gameManager.RefereeControllerList[i].HP.Value, gameManager.RefereeControllerList[i].HPLimit.Value);
                BlueUnitInfo[i-21].GetComponent<UnitInfoController>().SetLevel(gameManager.RefereeControllerList[i].Level.Value);
                BlueUnitInfo[i-21].GetComponent<UnitInfoController>().SetReviveTime(gameManager.RefereeControllerList[i].Reviving.Value, (gameManager.RefereeControllerList[i].MaxReviveProgress.Value - gameManager.RefereeControllerList[i].CurrentReviveProgress.Value) / gameManager.RefereeControllerList[i].ReviveProgressPerSec.Value);
            }
        }
        // Sync Observer UI
        // if (ObserverUI != null)
        // {
        //     ObserverUI.SetID(robotClass.Value == RobotClass.Sentry ? "AI" : RobotID.ToString());
        //     ObserverUI.SetHealth(HP.Value, HPLimit.Value);
        //     ObserverUI.SetLevelInfo(true, Level.Value);
        //     ObserverUI.SetBuff(ATKBuff.Value>0, CDBuff.Value>0, DEFBuff.Value>0, HealBuff.Value>0);
        // }

        // Update Mini Map
        if (NetworkManager.Singleton.LocalClient.PlayerObject != null) 
        {
            if (NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent<RefereeController>(out RefereeController referee))
            {
                userBelong = referee.faction.Value;
                userID = referee.RobotID.Value;
            } else {
                userBelong = Faction.Neu;
                userID = 0;
            }
        }

        foreach (var _unit in gameManager.RefereeControllerList.Values)
        {
            if (_unit.robotTags.Contains(RobotTag.Building)) continue;

            // Debug.Log($"[MatchInfo] userBelong {userBelong} _unit faction {_unit.faction}");

            if (userBelong == _unit.faction.Value && userID == _unit.RobotID.Value)
            {
                MiniMap.SetPoint(_unit.RobotID.Value, Faction.Self, _unit.Position, - _unit.Direction);
            } else if (userBelong == Faction.Neu || userBelong == _unit.faction.Value)
            {
                // Debug.Log($"[MatchInfo] Position {_unit.Position}");
                // Debug.Log($"[MatchInfo] Direction {_unit.Direction}");
                MiniMap.SetPoint(_unit.RobotID.Value, _unit.faction.Value, _unit.Position, - _unit.Direction);
            } else if (_unit.IsMarked) {
                // Debug.Log($"[MatchInfo] Marked Unit {_unit.RobotID.Value}");
                MiniMap.SetPoint(_unit.RobotID.Value, _unit.faction.Value, _unit.Position);
            }
        }
    }
}