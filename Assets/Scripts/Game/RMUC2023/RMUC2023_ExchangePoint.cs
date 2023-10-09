using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RMUC2023_ExchangePoint : AreaController 
{
    public ExchangePriceSO PriceInfo;
    public NetworkVariable<int> Level = new NetworkVariable<int>(0);
    public NetworkVariable<int> ExchangedCoin = new NetworkVariable<int>(0);

    public delegate void ExchangeAction(Faction faction, OreType type, int value);
    public static event ExchangeAction OnExchanged;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Capture()
    {
        base.Capture();

        foreach(var referee in RobotsInArea.Values)
        {
            if (referee.OreList.Count > 0 & referee.faction.Value == controllingFaction.Value & referee.robotClass.Value == RobotClass.Engineer)
            {
                var ore = referee.RemoveOre();
                if (ore != null)
                {
                    float factor = 1f;

                    if (ExchangedCoin.Value >= 1100) factor = 1.3f;
                    if (ExchangedCoin.Value >= 1625) factor = 2f;

                    switch(ore.Type.Value)
                    {
                        case OreType.Silver:
                            Destroy(ore.gameObject);
                            OnExchanged(belongFaction.Value, OreType.Silver, Mathf.RoundToInt(PriceInfo.silverPrice[Level.Value] * factor));
                            break;
                        case OreType.Gold:
                            Destroy(ore.gameObject);
                            OnExchanged(belongFaction.Value, OreType.Gold, Mathf.RoundToInt(PriceInfo.goldPrice[Level.Value] * factor));
                            break;
                        default:
                            Destroy(ore.gameObject);
                            break;
                    }
                }
            }
        }

        ResetCaptureProgress();
    }

    public int GetLeastLevel()
    {
        int i = -1;

        foreach (var limit in PriceInfo.levelLimit)
        {
            if (ExchangedCoin.Value >= limit) i ++;
        }

        return i;
    }

    public void SetLevel(int level)
    {
        if (level < GetLeastLevel())
        {
            Level.Value = GetLeastLevel();
        } else {
            Level.Value = level;
        }
    }
}