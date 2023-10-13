using UnityEngine;
using Unity.Netcode;

public class RMUC2023_GameManager : GameManager 
{
    [SerializeField] public new RMUC2023_OutPostController RedOutpost;
    [SerializeField] public new RMUC2023_OutPostController BlueOutpost;

    [SerializeField] AreaController RedControlPoint;
    [SerializeField] public NetworkVariable<float> RedControlTime = new NetworkVariable<float>(0);
    [SerializeField] AreaController BlueControlPoint;
    [SerializeField] public NetworkVariable<float> BlueControlTime = new NetworkVariable<float>(0);

    protected override void OnEnable()
    {
        base.OnEnable();
        
        RMUC2023_ExchangePoint.OnExchanged += ExchangeUpload;
    }

    protected override void Update()
    {
        base.Update();

        if (RedControlPoint.Occupied.Value & !BlueControlPoint.Occupied.Value)
        {
            BlueControlTime.Value = 0f;
            RedOutpost.Suppressed.Value = false;

            if (RedControlTime.Value < 6)
            {
                RedControlTime.Value += Time.deltaTime;
            } else {
                BlueOutpost.Suppressed.Value = true;
            }
        } else if (!RedControlPoint.Occupied.Value & BlueControlPoint.Occupied.Value)
        {
            RedControlTime.Value = 0f;
            BlueOutpost.Suppressed.Value = false;

            if (BlueControlTime.Value < 6)
            {
                BlueControlTime.Value += Time.deltaTime;
            } else {
                RedOutpost.Suppressed.Value = true;
            }
        } else if (!RedControlPoint.Occupied.Value & !BlueControlPoint.Occupied.Value) 
        {
            BlueControlTime.Value = 0f;
            BlueOutpost.Suppressed.Value = false;
            RedControlTime.Value = 0f;
            RedOutpost.Suppressed.Value = false;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isRunning.Value)
        {
            if ((TimeLeft.Value < 240) & !RedOutpost.Stopped.Value & RedOutpost.Enabled.Value) RedOutpost.Stop();
            if ((TimeLeft.Value < 240) & !BlueOutpost.Stopped.Value & BlueOutpost.Enabled.Value) BlueOutpost.Stop();

            if (TimeLeft.Value >= 240)
            {
                if (RedOutpost.HPLimit.Value - RedOutpost.HP.Value >= 500 & !RedOutpost.HasGivenEXP)
                {
                    RedOutpost.HasGivenEXP = true;

                    int _id = RedOutpost.GetLastAttacker();
                    if (_id != 0)
                    {
                        RefereeControllerList[_id].EXP.Value += 25;
                    } else {
                        DistributeEXP(Faction.Blue, 25);
                    }
                }

                if (BlueOutpost.HPLimit.Value - BlueOutpost.HP.Value >= 500 & !BlueOutpost.HasGivenEXP)
                {
                    BlueOutpost.HasGivenEXP = true;

                    int _id = BlueOutpost.GetLastAttacker();
                    if (_id != 0)
                    {
                        RefereeControllerList[_id].EXP.Value += 25;
                    } else {
                        DistributeEXP(Faction.Red, 25);
                    }
                }
            }
        }
    }

    #region Coin

    [SerializeField] private RMUC2023_ExchangePoint RedExchangeStation;
    [SerializeField] private RMUC2023_ExchangePoint BlueExchangeStation;

    protected override void OnTimeLeftChange(float oldTime, float newTime)
    {
        
        if (oldTime >= 30.0f && newTime < 30.0f) 
        {
            // Big Buff
        }
        if (oldTime >= 60.0f && newTime < 60.0f) 
        {
            AddCoin(Faction.Red, 150);
            AddCoin(Faction.Blue, 150);
        }
        if (oldTime >= 74.0f && newTime < 74.0f) 
        {
            // Stop Big Buff
        }
        if (oldTime >= 104.0f && newTime < 104.0f)
        {
            // Big Buff
        }
        if (oldTime >= 120.0f && newTime < 120.0f)
        {
            AddCoin(Faction.Red, 50);
            AddCoin(Faction.Blue, 50);
        }
        if (oldTime >= 150.0f && newTime < 150.0f)
        {
            // Stop Big Buff
        }
        if (oldTime >= 180.0f && newTime < 180.0f)
        {
            AddCoin(Faction.Red, 50);
            AddCoin(Faction.Blue, 50);
            // Big Buff
        }
        if (oldTime >= 240.0f && newTime < 240.0f)
        {
            AddCoin(Faction.Red, 50);
            AddCoin(Faction.Blue, 50);
            // Stop Big Buff
        }
        if (oldTime >= 270.0f && newTime < 270.0f)
        {
            // Small Buff
        }
        if (oldTime >= 300.0f && newTime < 300.0f)
        {
            AddCoin(Faction.Red, 50);
            AddCoin(Faction.Blue, 50);
        }
        if (oldTime >= 330.0f && newTime < 330.0f)
        {
            // Stop Small Buff
        }
        if (oldTime >= 360.0f && newTime < 360.0f)
        {
            AddCoin(Faction.Red, 50);
            AddCoin(Faction.Blue, 50);
            // Small Buff
        }
    }

    void ExchangeUpload(Faction faction, OreType type, int value)
    {
        ExchangeHandlerServerRpc(faction, type, value);
    }

    [ServerRpc]
    void ExchangeHandlerServerRpc(Faction faction, OreType type, int value, ServerRpcParams serverRpcParams = default)
    {
        int coin = value;

        if (type == OreType.Gold && !HasFirstGold.Value)
        {
            coin += 250;
            HasFirstGold.Value = true;
        }

        AddCoin(faction, coin);
    }

    #endregion

    #region Hero Snipe

    [SerializeField] private BuffEffectSO HeroSnipeBuff;

    [ServerRpc]
    protected override void ShootHandlerServerRpc(int shooterID, int shooterType, int robotID, ServerRpcParams serverRpcParams = default)
    {
        // base.ShootHandlerServerRpc(shooterID, shooterType, robotID);

        RefereeController referee = RefereeControllerList[robotID];

        if (referee.robotClass.Value == RobotClass.Hero & referee.HasBuff(HeroSnipeBuff))
        {
            AddCoin(referee.faction.Value, 10);
        }
    }

    #endregion

    #region Engineer Initial Buff

    [SerializeField] private BuffEffectSO EngineerInitBuff;

    public override void StartGame()
    {
        foreach(var _referee in RefereeControllerList.Values)
        {
            if (_referee.robotClass.Value == RobotClass.Engineer) _referee.AddBuff(EngineerInitBuff);
        }

        base.StartGame();
    }

    #endregion
}