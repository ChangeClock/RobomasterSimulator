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

    [SerializeField] private BuffEffectSO HeroSnipeBuff;

    protected override void Update()
    {
        base.Update();

        if (RedControlPoint.Occupied.Value & !BlueControlPoint.Occupied.Value)
        {
            BlueControlTime.Value = 0f;

            if (RedControlTime.Value < 6)
            {
                RedControlTime.Value += Time.deltaTime;
            } else {
                BlueOutpost.Suppressed.Value = true;
            }
        } else if (!RedControlPoint.Occupied.Value & BlueControlPoint.Occupied.Value)
        {
            RedControlTime.Value = 0f;

            if (BlueControlTime.Value < 6)
            {
                BlueControlTime.Value += Time.deltaTime;
            } else {
                RedOutpost.Suppressed.Value = true;
            }
        } else if (!RedControlPoint.Occupied.Value & !BlueControlPoint.Occupied.Value) 
        {
            BlueControlTime.Value = 0f;
            RedControlTime.Value = 0f;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isRunning.Value)
        {
            if (TimeLeft.Value < 240)
            {
                RedOutpost.Stopped.Value = true;
                BlueOutpost.Stopped.Value = true;
            }
        }
    }

    [ServerRpc]
    protected override void ShootHandlerServerRpc(int shooterID, int shooterType, int robotID, ServerRpcParams serverRpcParams = default)
    {
        // base.ShootHandlerServerRpc(shooterID, shooterType, robotID);

        RefereeController referee = RefereeControllerList[robotID];

        if (referee.robotClass.Value == RobotClass.Hero & referee.HasBuff(HeroSnipeBuff))
        {
            switch (referee.faction.Value)
            {
                case Faction.Red:
                    RedCoin.Value += 10;
                    break;
                case Faction.Blue:
                    BlueCoin.Value += 10;
                    break;
                default :
                    break;
            }
        }
    }
}