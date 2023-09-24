using System;
using UnityEngine;
using TMPro;

public class UnitInfoBar : MonoBehaviour 
{
    [SerializeField] BarController HealthBar;
    [SerializeField] TextMeshProUGUI ID;
    [SerializeField] GameObject LevelInfo;
    [SerializeField] GameObject[] LevelIcons;
    [SerializeField] GameObject ATKBuff;
    [SerializeField] GameObject CDBuff;
    [SerializeField] GameObject DEFBuff;
    [SerializeField] GameObject HealBuff;

    public void SetHealth(float health, float maxHealth)
    {
        HealthBar.SetMaxValue(maxHealth);
        HealthBar.SetValue(health);
    }

    public void SetID(String id)
    {
        ID.text = id;
    }

    public void SetLevelInfo(bool active, int level = 0)
    {
        LevelInfo.SetActive(active);

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

    public void SetBuff(bool atkActive, bool cdActive, bool defActive, bool healActive)
    {
        ATKBuff.SetActive(atkActive);
        CDBuff.SetActive(cdActive);
        DEFBuff.SetActive(defActive);
        HealBuff.SetActive(healActive);
    }
}