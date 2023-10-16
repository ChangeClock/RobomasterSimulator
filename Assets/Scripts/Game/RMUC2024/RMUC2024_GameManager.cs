using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RMUC2024_GameManager : GameManager 
{
    [SerializeField] public new RMUC2023_OutPostController RedOutpost;
    [SerializeField] public new RMUC2023_OutPostController BlueOutpost;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        RMUC2023_ExchangePoint.OnExchanged += ExchangeUpload;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        RMUC2023_ExchangePoint.OnExchanged -= ExchangeUpload;
    }

    protected override void Update()
    {
        base.Update();

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
                        RefereeControllerList[_id].EXP.Value += 100;
                    } else {
                        DistributeEXP(Faction.Blue, 100);
                    }
                }

                if (BlueOutpost.HPLimit.Value - BlueOutpost.HP.Value >= 500 & !BlueOutpost.HasGivenEXP)
                {
                    BlueOutpost.HasGivenEXP = true;

                    int _id = BlueOutpost.GetLastAttacker();
                    if (_id != 0)
                    {
                        RefereeControllerList[_id].EXP.Value += 100;
                    } else {
                        DistributeEXP(Faction.Red, 100);
                    }
                }
            }
        }
    }

    #region Coin & Buff

    [SerializeField] private RMUC2024_ExchangePoint RedExchangeStation;
    [SerializeField] private RMUC2024_ExchangePoint BlueExchangeStation;

    [SerializeField] private List<RMUC2023_MineMountain> Mines = new List<RMUC2023_MineMountain>();
    [SerializeField] private List<AreaController> MineBuffAreas = new List<AreaController>();

    [SerializeField] private List<AreaController> HighLands = new List<AreaController>();
    [SerializeField] private List<AreaController> BaseAreas = new List<AreaController>();
    [SerializeField] private List<AreaController> BoostPoints = new List<AreaController>();

    [SerializeField] private BuffEffectSO HighLandBuff_CD200;
    [SerializeField] private BuffEffectSO HighLandBuff_CD300;
    [SerializeField] private BuffEffectSO HighLandBuff_CD500;

    [SerializeField] private BuffEffectSO BaseBuff;
    [SerializeField] private BuffEffectSO BaseBuff_CD200;
    [SerializeField] private BuffEffectSO BaseBuff_CD300;
    [SerializeField] private BuffEffectSO BaseBuff_CD500;

    [SerializeField] private BuffEffectSO BoostBuff;
    [SerializeField] private BuffEffectSO BoostBuff_CD200;
    [SerializeField] private BuffEffectSO BoostBuff_CD300;
    [SerializeField] private BuffEffectSO BoostBuff_CD500;

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

            foreach (var area in HighLands)
            {
                area.RemoveBuff(HighLandBuff_CD300);
                area.AddBuff(HighLandBuff_CD500);
            }

            foreach (var area in BaseAreas)
            {
                area.RemoveBuff(BaseBuff_CD300);
                area.AddBuff(BaseBuff_CD500);
            }
            
            foreach (var area in BoostPoints)
            {
                area.RemoveBuff(BoostBuff_CD300);
                area.AddBuff(BoostBuff_CD500);
            }

            // Big Buff
        }
        if (oldTime >= 240.0f && newTime < 240.0f)
        {
            AddCoin(Faction.Red, 50);
            AddCoin(Faction.Blue, 50);

            foreach (var area in HighLands)
            {
                area.RemoveBuff(HighLandBuff_CD200);
                area.AddBuff(HighLandBuff_CD300);
            }

            foreach (var area in BaseAreas)
            {
                area.RemoveBuff(BaseBuff_CD200);
                area.AddBuff(BaseBuff_CD300);
            }
            
            foreach (var area in BoostPoints)
            {
                area.RemoveBuff(BoostBuff_CD200);
                area.AddBuff(BoostBuff_CD300);
            }

            // Stop Small Buff
        }
        if (oldTime >= 270.0f && newTime < 270.0f)
        {
            // Small Buff
        }
        if (oldTime >= 300.0f && newTime < 300.0f)
        {
            AddCoin(Faction.Red, 50);
            AddCoin(Faction.Blue, 50);

            foreach (var area in HighLands)
            {
                area.AddBuff(HighLandBuff_CD200);
            }

            foreach (var area in BaseAreas)
            {
                area.RemoveBuff(BaseBuff);
                area.AddBuff(BaseBuff_CD200);
            }
            
            foreach (var area in BoostPoints)
            {
                area.RemoveBuff(BoostBuff);
                area.AddBuff(BoostBuff_CD200);
            }

            foreach (var area in MineBuffAreas)
            {
                area.Enabled.Value = false;
            }
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

    #region Hero Snipe & Shoot EXP

    [SerializeField] private BuffEffectSO HeroSnipeBuff;

    [ServerRpc]
    protected override void ShootHandlerServerRpc(int shooterID, int shooterType, int robotID, ServerRpcParams serverRpcParams = default)
    {
        RefereeController referee = RefereeControllerList[robotID];

        if (referee.robotClass.Value == RobotClass.Hero & referee.HasBuff(HeroSnipeBuff))
        {
            AddCoin(referee.faction.Value, 10);
        }

        switch (shooterType)
        {
            case 0:
                AddEXP(referee, 1);
                break;
            case 1:
                AddEXP(referee, 10);
                break;
            default:
                break;
        }
    }

    #endregion

    #region Damage EXP

    [ServerRpc]
    protected override void DamageHandlerServerRpc(int damageType, float damage, int armorID, int attackerID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        base.DamageHandlerServerRpc(damageType, damage, armorID, attackerID, robotID);
        
        // Debug.Log($"[GameManager] AttackerID {attackerID} {RefereeControllerList.ContainsKey(attackerID)}");
        // Debug.Log($"[GameManager] VictimID {robotID} {RefereeControllerList.ContainsKey(robotID)}");

        if (attackerID == 0 || !RefereeControllerList.ContainsKey(attackerID) || !RefereeControllerList.ContainsKey(robotID)) return;

        RefereeController attacker = RefereeControllerList[attackerID];
        RefereeController victim = RefereeControllerList[robotID];

        // Debug.Log($"[GameManager] Attacker Faction {attacker.faction.Value}");
        // Debug.Log($"[GameManager] Victim Faction {victim.faction.Value}");

        // Friendly Fire !!!!
        if (attacker.faction.Value == victim.faction.Value) return;

        switch (damageType)
        {
            case 1:
                AddEXP(attacker, 4 * damage);
                break;
            case 2:
                AddEXP(attacker, 1 * damage);
                break;
            case 3:
                // Missle damage
                // TODO: Add EXP to attackerID belong team
                break;
            default:
                Debug.LogWarning("Unknown Damage Type" + damageType);
                break;
        }
    }

    #endregion

    #region Engineer Initial Buff & Sentry Initial Level

    [SerializeField] private BuffEffectSO EngineerInitBuff;

    public override void StartGame()
    {
        RedExchangeStation.Reset();
        BlueExchangeStation.Reset();

        foreach (var mine in Mines)
        {
            mine.ResetOre();
        }

        foreach(var _referee in RefereeControllerList.Values)
        {
            if (_referee.robotClass.Value == RobotClass.Engineer) _referee.AddBuff(EngineerInitBuff);
        }

        if (RedSentry != null) RedSentry.Level.Value = 10;
        if (BlueSentry != null) BlueSentry.Level.Value = 10;

        foreach (var area in HighLands)
        {
            area.RemoveBuff(HighLandBuff_CD200);
            area.RemoveBuff(HighLandBuff_CD300);
            area.RemoveBuff(HighLandBuff_CD500);
        }

        foreach (var area in BaseAreas)
        {
            area.AddBuff(BaseBuff);
            area.RemoveBuff(BaseBuff_CD200);
            area.RemoveBuff(BaseBuff_CD300);
            area.RemoveBuff(BaseBuff_CD500);
        }
        
        foreach (var area in BoostPoints)
        {
            area.AddBuff(BoostBuff);
            area.RemoveBuff(BoostBuff_CD200);
            area.RemoveBuff(BoostBuff_CD300);
            area.RemoveBuff(BoostBuff_CD500);
        }

        foreach (var area in MineBuffAreas)
        {
            area.Enabled.Value = true;
        }

        base.StartGame();
    }

    #endregion

    #region EXP & GameLogic

    [ServerRpc]
    protected override void DeathHandlerServerRpc(int attackerID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        RefereeController Attacker = RefereeControllerList.ContainsKey(attackerID) ? RefereeControllerList[attackerID] : null;
        RefereeController Victim = RefereeControllerList[robotID];

        switch (robotID)
        {
            // TODO: Change Base Status
            case 7:
                if (!RedOutpost.Enabled.Value) BaseShieldOff(Faction.Red);
                break;
            case 27:
                if (!BlueOutpost.Enabled.Value) BaseShieldOff(Faction.Blue);
                break;

            // TODO: Change Sentry & Base Status
            case 18:
                if (RedSentry == null || !RedSentry.Enabled.Value) 
                {
                    BaseShieldOff(Faction.Red);
                } else {
                    RedSentry.Immutable.Value = false;
                    RedSentry.HP.Value += 600;
                }
                break;
            case 38:
                if (BlueSentry == null || !BlueSentry.Enabled.Value)
                {
                    BaseShieldOff(Faction.Blue);
                } else {
                    BlueSentry.Immutable.Value = false;
                    BlueSentry.HP.Value += 600;
                }
                break;
            
            // TODO: Stop Game Handler
            case 19:
                FinishGame(Faction.Blue);
                break;
            case 39:
                FinishGame(Faction.Red);
                break;
            
            default:
                // TODO: If the robot is penalty to death, disable revival
                Victim.MaxReviveProgress.Value = Mathf.RoundToInt(10.0f + (420.0f - TimeLeft.Value) / 10.0f);
                Victim.PurchaseRevivePrice.Value = Mathf.RoundToInt(((420 - TimeLeft.Value) / 60) * 100 + Victim.Level.Value * 50);
                Victim.Reviving.Value = true;
                Victim.Enabled.Value = false;
                break;
        }

        if (!Victim.robotTags.Contains(RobotTag.GroundUnit)) return;

        List<int> AssistantIDs = new List<int>();

        if (Victim.AttackList.Count > 0)
        {
            foreach (var _id in Victim.AttackList.Keys)
            {
                if (RefereeControllerList[_id].faction.Value != Victim.faction.Value && _id != attackerID) 
                {
                    AssistantIDs.Add(_id);
                }
            }
        }

        int levelDelta = 0;
        
        if (Attacker != null)
        {
            if (Attacker.faction.Value != Victim.faction.Value & Attacker.robotTags.Contains(RobotTag.GrowingUnit)) 
            {
                levelDelta = Victim.Level.Value - Attacker.Level.Value;
                Attacker.EXP.Value += Mathf.RoundToInt(50 * Victim.Level.Value * (1 + 0.2f * (levelDelta > 0 ? levelDelta : 0)) * (1 - 0.1f * AssistantIDs.Count));
            }
        }

        if (AssistantIDs.Count > 0)
        {
            foreach (var _id in AssistantIDs)
            {
                RefereeControllerList[_id].EXP.Value += Mathf.RoundToInt(50 * Victim.Level.Value * (1 + 0.2f * (levelDelta > 0 ? levelDelta : 0)) * 0.1f);
            }
        }

        if (!HasFirstBlood.Value)
        {
            HasFirstBlood.Value = true;
        }
    }

    public void AddEXP(RefereeController referee, float exp)
    {
        float _exp = exp;

        // TODO: Boost Buff
        if (referee.HasBuff(BoostBuff))
        {
            _exp *= 2f;
        }

        // TODO: Small Buff

        // TODO: Half Auto Buff

        // TODO: Balance Buff
        if (referee.robotTags.Contains(RobotTag.Balance)) _exp *= 1.5f;

        Debug.Log($"[GameManager] Add {_exp} EXP");

        referee.EXP.Value += Mathf.RoundToInt(_exp);
    }

    #endregion
}