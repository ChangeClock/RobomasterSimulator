using Unity.Netcode;
using UnityEngine;

public class RMUC2024_RepairArea : AreaController 
{
    void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (!Enabled.Value) return;

        // if (printLog) Debug.Log($"[AreaController] {ID} Area contains {RobotsInArea.Count} robots");

        if (RobotsInArea.Count > 0)
        {
            foreach (var _referee in RobotsInArea.Values)
            {
                if(_referee.robotTags.Contains(RobotTag.GroundUnit)) _referee.ShooterEnabled.Value = true;
            }
        }
    }
}