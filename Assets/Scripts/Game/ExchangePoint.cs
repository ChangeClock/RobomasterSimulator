using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ExchangePoint : AreaController 
{
    public ExchangePriceSO PriceInfo;

    // 0: Idle 1: Exchanging 2: Resetting
    public NetworkVariable<int> Status = new NetworkVariable<int>(0); 
    public NetworkVariable<int> Level = new NetworkVariable<int>(0);

    public NetworkVariable<float> WaitTime = new NetworkVariable<float>(0);
    public NetworkVariable<float> LossRatio = new NetworkVariable<float>(0);

    public NetworkVariable<int> ExchangedCoin = new NetworkVariable<int>(0);

    public delegate void ExchangeAction(Faction faction, OreType type, int value);
    public static event ExchangeAction OnExchanged;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    void Update()
    {
        if (ResetProgress.Value < MaxResetProgress.Value)
        { 
            Status.Value = 2;
        } else {
            Status.Value = 0;
        }

        if (Status.Value != 1)
        {
            WaitTime.Value = 0;
            LossRatio.Value = 0;
            CaptureProgress.Value = 0;
        } else {
            WaitTime.Value += Time.deltaTime;
            if (WaitTime.Value > 15 & WaitTime.Value <= 50) 
            {
                LossRatio.Value = 0.02f * (WaitTime.Value - 15);
            } else if (WaitTime.Value > 50) {
                LossRatio.Value = 1;
            }
        }
    }

    protected override void Capture()
    {
        if (Status.Value != 1) return;

        base.Capture();

        foreach(var referee in RobotsInArea.Values)
        {
            if (referee.OreList.Count > 0 & referee.faction.Value == controllingFaction.Value & referee.robotClass.Value == RobotClass.Engineer)
            {
                var ore = referee.RemoveOre();
                if (ore != null)
                {
                    float factor = 1f;

                    if (PriceInfo.levelLimit.Length > 0 && PriceInfo.levelBonus.Length > 0)
                    {
                        int i = 0;
                        foreach (var limit in PriceInfo.levelLimit)
                        {
                            if (ExchangedCoin.Value >= limit) 
                            {
                                factor = PriceInfo.levelBonus[i];
                            } else {
                                continue;
                            }
                            i ++;
                        }
                    }

                    float loss = 0f;
                    float price = 0f;
                    float lastLevelPrice = 0f;

                    switch(ore.Type.Value)
                    {
                        case OreType.Silver:
                            price = PriceInfo.silverPrice[Level.Value];
                            if (Level.Value > 0) lastLevelPrice = PriceInfo.silverPrice[Level.Value-1];
                            break;
                        case OreType.Gold:
                            price = PriceInfo.goldPrice[Level.Value];
                            if (Level.Value > 0) lastLevelPrice = PriceInfo.goldPrice[Level.Value-1];
                            break;
                        default:
                            break;
                    }

                    if (Level.Value > 0) loss = (price - lastLevelPrice) * LossRatio.Value;

                    Destroy(ore.gameObject);
                    Status.Value = 2;
                    WaitTime.Value = 0;
                    OnExchanged(belongFaction.Value, ore.Type.Value, Mathf.RoundToInt((price - loss) * factor));
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

    public void SetLevel(bool enable, int level)
    {
        ResetCaptureProgress();

        if (!enable)
        {
            Status.Value = 0;
        } else {
            Status.Value = 1;
        }

        if (level < GetLeastLevel())
        {
            Level.Value = GetLeastLevel();
        } else {
            Level.Value = level;
        }
    }

    public override void Reset()
    {
        base.Reset();

        Status.Value = 0;
        WaitTime.Value = 0;
        LossRatio.Value = 0;
        ExchangedCoin.Value = 0;
    }
}