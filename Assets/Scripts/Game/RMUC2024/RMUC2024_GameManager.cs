using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RMUC2024_GameManager : GameManager 
{
    [SerializeField] public new RMUC2023_OutPostController RedOutpost;
    [SerializeField] public new RMUC2023_OutPostController BlueOutpost;

    protected override void Start()
    {
        base.Start();

        foreach(var fac in Factions)
        {
            SmallBuffAdditionalEXP.Add(0);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        RMUC2023_BoostPoint.OnBoost += BoostHandler;
        BuffController.OnActive += ActivateHandler;
        RefereeController.OnMark += MarkUploadHandler;
        RefereeController.OnMarkReset += MarkResetUploadHandler;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        RMUC2023_BoostPoint.OnBoost -= BoostHandler;
        BuffController.OnActive -= ActivateHandler;
        RefereeController.OnMark -= MarkUploadHandler;
        RefereeController.OnMarkReset -= MarkResetUploadHandler;
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
            ToggleBuff(true, BuffType.Big);
            // Big Buff
        }
        if (oldTime >= 60.0f && newTime < 60.0f) 
        {
            AddCoin(Faction.Red, 150);
            AddCoin(Faction.Blue, 150);
        }
        if (oldTime >= 74.0f && newTime < 74.0f) 
        {
            ToggleBuff(false, BuffType.Big);
            // Stop Big Buff
        }
        if (oldTime >= 104.0f && newTime < 104.0f)
        {
            ToggleBuff(true, BuffType.Big);
            // Big Buff
        }
        if (oldTime >= 120.0f && newTime < 120.0f)
        {
            AddCoin(Faction.Red, 50);
            AddCoin(Faction.Blue, 50);
        }
        if (oldTime >= 150.0f && newTime < 150.0f)
        {
            ToggleBuff(false, BuffType.Big);
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

            ToggleBuff(true, BuffType.Big);
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
            
            ToggleBuff(false, BuffType.Small);

            // Stop Small Buff
        }
        if (oldTime >= 270.0f && newTime < 270.0f)
        {
            ToggleBuff(true, BuffType.Small);
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
            ToggleBuff(false, BuffType.Small);
            // Stop Small Buff
        }
        if (oldTime >= 360.0f && newTime < 360.0f)
        {
            AddCoin(Faction.Red, 50);
            AddCoin(Faction.Blue, 50);
            ToggleBuff(true, BuffType.Small);
            // Small Buff
        }
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

        float factor = 1f;

        if (!victim.robotTags.Contains(RobotTag.Building)) factor = 4f;

        if (!attacker.robotTags.Contains(RobotTag.GrowingUnit))
        {
            AddEXP(attacker, damage * factor);
        } else {
            DistributeEXP(attacker.faction.Value, damage * factor);
        }
    }

    #endregion

    #region Engineer Initial Buff & Sentry Initial Level

    [SerializeField] private BuffEffectSO EngineerInitBuff;

    public override void StartGame()
    {
        base.StartGame();

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
    }

    #endregion

    #region EXP & GameLogic

    [SerializeField] BuffEffectSO SmallBuff;
    [SerializeField] BuffEffectSO BigBuff;
    [SerializeField] NetworkList<float> SmallBuffAdditionalEXP = new NetworkList<float>();

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
                Victim.MaxReviveProgress.Value = (int)Math.Round(10.0f + (420.0f - TimeLeft.Value) / 10.0f);
                Victim.PurchaseRevivePrice.Value = (int)Math.Round(((420 - TimeLeft.Value) / 60) * 80 + (Victim.Level.Value + 1) * 20);
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

    [ServerRpc]
    protected override void PurchaseHandlerServerRpc(int id, PurchaseType type, int amount, ServerRpcParams serverRpcParams = default)
    {
        RefereeController referee = RefereeControllerList[id];
        Faction faction = referee.faction.Value;

        int cost = 0;

        switch (type)
        {
            case PurchaseType.Remote_HP:
                cost = 50 + Mathf.CeilToInt((420 - TimeLeft.Value) / 60) * 20;
                if (CostCoin(faction, cost)) 
                {
                    RemoteHPTimes[(int)faction] --;
                    StartCoroutine(RemoteHealthSupply(referee));
                }
                break;
            case PurchaseType.Remote_Ammo0:
                cost = 150;
                if (CostCoin(faction, cost)) 
                {
                    RemoteAmmo0Times[(int)faction] --;
                    StartCoroutine(RemoteAmmo0Supply(referee));
                }
                break;
            case PurchaseType.Remote_Ammo1:
                cost = 200;
                if (CostCoin(faction, cost)) 
                {
                    RemoteAmmo1Times[(int)faction] --;
                    StartCoroutine(RemoteAmmo1Supply(referee));
                }
                break;
            case PurchaseType.Ammo0:
                cost = amount;
                if (CostCoin(faction, cost)) 
                {
                    Ammo0Supply[(int)faction] -= amount;
                    referee.Ammo0.Value += amount;
                }
                break;
            case PurchaseType.Ammo1:
                cost = amount * 15;
                if (CostCoin(faction, cost)) 
                {
                    Ammo1Supply[(int)faction] -= amount;
                    referee.Ammo1.Value += amount;
                }
                break;
            default:
                break;
        }
    }

    void BoostHandler(RefereeController referee)
    {
        if (referee.robotTags.Contains(RobotTag.GroundUnit) & !referee.HasBoostEXP.Value)
        {
            referee.HasBoostEXP.Value = true;
            AddEXP(referee, 300);
        }
    }

    void ActivateHandler(Faction faction, BuffType type, int totalScore)
    {
        Debug.Log($"[RMUC2024_GameManager] Activate {faction} {type} with {totalScore} score.");
        ToggleBuff(false, BuffType.Small);

        switch (type)
        {
            case BuffType.Small:
                AddFactionBuff(faction, SmallBuff);
                break;
            case BuffType.Big:
                int atk = 50;
                int def = 25;
                if (totalScore > 15 & totalScore <= 25)
                {
                    atk = 55;
                } else if (totalScore > 25 & totalScore <= 35)
                {
                    atk = 60;
                } else if (totalScore > 35 & totalScore <= 40)
                {
                    atk = 100;
                } else if (totalScore > 40 & totalScore <= 45)
                {
                    atk = 200;
                } else if (totalScore == 46)
                {
                    atk = 240;
                    def = 30;
                } else if (totalScore == 47)
                {
                    atk = 280;
                    def = 35;
                } else if (totalScore == 48)
                {
                    atk = 320;
                    def = 40;
                } else if (totalScore == 49)
                {
                    atk = 360;
                    def = 45;
                } else if (totalScore == 50)
                {
                    atk = 400;
                    def = 50;
                }
                BuffEffectSO BigBuff = new BuffEffectSO(45000, def, atk);
                AddFactionBuff(faction, BigBuff);
                break;
            default:
                break;
        }
    }

    void AddFactionBuff(Faction faction, BuffEffectSO buff)
    {
        foreach(var referee in RefereeControllerList.Values)
        {
            if (referee.faction.Value == faction & !referee.robotTags.Contains(RobotTag.Building)) referee.AddBuff(buff);
        }
    }

    public void AddEXP(RefereeController referee, float exp)
    {
        float _exp = exp;

        // TODO: Small Buff
        if (referee.HasBuff(SmallBuff) & SmallBuffAdditionalEXP[(int)referee.faction.Value] < 800) 
        {
            SmallBuffAdditionalEXP[(int)referee.faction.Value] += _exp;
            _exp *= 2;
        }

        // TODO: Half Auto Buff

        // TODO: Balance Buff
        if (referee.robotTags.Contains(RobotTag.Balance)) _exp *= 1.5f;

        Debug.Log($"[GameManager] Add {_exp} EXP");

        referee.EXP.Value += Mathf.RoundToInt(_exp);
    }

    void ResetSmallBuffAddtionalEXP()
    {
        foreach(var fac in Factions)
        {
            SmallBuffAdditionalEXP[(int)fac] = 0;
        }
    }

    #endregion

    #region Mark

    [SerializeField] private BuffEffectSO MarkedBuff;

    void MarkUploadHandler(int robotID, int markID, Vector2 markPostion)
    {
        MarkHandlerServerRpc(robotID, markID, markPostion);
    }

    void MarkResetUploadHandler(int robotID)
    {
        MarkResetHandlerServerRpc(robotID);
    }

    [ServerRpc]
    void MarkHandlerServerRpc(int robotID, int markID, Vector2 markPostion, ServerRpcParams serverRpcParams = default)
    {
        // Debug.Log($"[RMUC2024_GameManager] MarkHandlerServerRpc {robotID} {markID} {markPostion}");

        // Debug.Log($"[RMUC2024_GameManager] MarkHandlerServerRpc {RefereeControllerList.ContainsKey(robotID)} {RefereeControllerList.ContainsKey(markID)}");

        if (!RefereeControllerList.ContainsKey(robotID)) return;
        if (!RefereeControllerList.ContainsKey(markID)) return;
        
        RefereeController referee = RefereeControllerList[robotID];
        RefereeController markedUnit = RefereeControllerList[markID];

        // Debug.Log($"[RMUC2024_GameManager] MarkHandlerServerRpc {referee.robotClass.Value} {markedUnit.faction.Value} {referee.faction.Value}");

        if (referee.robotClass.Value != RobotClass.Lidar) return;
        if (markedUnit.faction.Value == referee.faction.Value) return;

        float error = Vector2.Distance(markPostion, markedUnit.Position);
        float gainedProgress = 0f;

        // Debug.Log($"[RMUC2024_GameManager] MarkHandlerServerRpc {error}");

        if (markedUnit.IsMarked) 
        {
            markedUnit.AddBuff(MarkedBuff);
        }

        markedUnit.CurrentMarkResetProgress.Value = 0f;

        if (markedUnit.CurrentMarkProgress.Value >= markedUnit.MaxMarkProgress.Value & error < 1.6) return;
        if (markedUnit.CurrentMarkProgress.Value <= 0f & error >= 1.6) return;

        if (error < 0.8)
        {
            if (markedUnit.LastMarkProgress.Value < 0f) markedUnit.LastMarkProgress.Value = 0f;
            gainedProgress = 1 + markedUnit.LastMarkProgress.Value;
            markedUnit.CurrentMarkProgress.Value += gainedProgress;
            markedUnit.LastMarkProgress.Value = gainedProgress;
        } else if (error < 1.6)
        {
            if (markedUnit.LastMarkProgress.Value < 0f) markedUnit.LastMarkProgress.Value = 0f;
            gainedProgress = 0.5f + markedUnit.LastMarkProgress.Value;
            markedUnit.CurrentMarkProgress.Value += gainedProgress;
            markedUnit.LastMarkProgress.Value = gainedProgress;
        } else {
            gainedProgress = -0.8f + markedUnit.LastMarkProgress.Value;
            markedUnit.CurrentMarkProgress.Value += gainedProgress;
            markedUnit.LastMarkProgress.Value = 0f;
        }

        if (markedUnit.CurrentMarkProgress.Value <= 0f) markedUnit.CurrentMarkProgress.Value = 0f;
        if (markedUnit.CurrentMarkProgress.Value >= markedUnit.MaxMarkProgress.Value) markedUnit.CurrentMarkProgress.Value = markedUnit.MaxMarkProgress.Value;
    }

    [ServerRpc]
    void MarkResetHandlerServerRpc(int robotID, ServerRpcParams serverRpcParams = default)
    {
        if (!RefereeControllerList.ContainsKey(robotID)) return;
        
        RefereeController referee = RefereeControllerList[robotID];

        float gainedProgress = -0.8f + referee.LastMarkProgress.Value;
        referee.CurrentMarkProgress.Value += gainedProgress;
        referee.LastMarkProgress.Value = 0f;
    }

    #endregion
}