using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RMUC2023_MineArea : AreaController
{
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
}