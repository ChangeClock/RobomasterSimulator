using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RMUC2023_MineArea : AreaController
{
    public OreType Type = OreType.Silver;

    protected override void Capture()
    {
        if (RobotsInArea.Count > 0)
        {
            foreach (var referee in RobotsInArea.Values)
            {
                if (referee.faction.Value == controllingFaction.Value & referee.robotClass.Value == RobotClass.Engineer)
                {
                    CaptureEvent(referee);
                    ResetCaptureProgress();
                    return;
                }
            }
        }

        ResetCaptureProgress();
        controllingFaction.Value = Faction.Neu;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        RefereeController robot = other.GetComponentInParent<RefereeController>();

        MaxCaptureProgress.Value = (Type == OreType.Silver) ? robot.MineSilverSpeed.Value : robot.MineGoldSpeed.Value;
    } 
}