using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AreaController : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<int> ID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // 0: not occupied 1: Blue 2: Red
    [SerializeField] private NetworkVariable<bool> Enabled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private bool printLog = false;
    
    [SerializeField] private NetworkVariable<Faction> belongFaction = new NetworkVariable<Faction>(Faction.Neu);
    [SerializeField] private NetworkVariable<bool> isControlPoint = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<bool> Occupied = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<Faction> controllingFaction = new NetworkVariable<Faction>(Faction.Neu);

    [SerializeField] private NetworkVariable<float> MaxControlProgress = new NetworkVariable<float>(100.0f);
    [SerializeField] private NetworkVariable<float> ControlProgress = new NetworkVariable<float>(0.0f);
    [SerializeField] private NetworkVariable<float> ControlProgressPerSecond = new NetworkVariable<float>(1.0f);

    [SerializeField] private BuffEffectSO[] BuffList;

    private Dictionary<int, RefereeController> RobotsInArea = new Dictionary<int, RefereeController>();

    void Start()
    {
       
    }

    void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        if (!Enabled.Value) return;

        if (printLog) Debug.Log($"[AreaController] {ID} Area contains {RobotsInArea.Count} robots");

        if (RobotsInArea.Count > 0)
        {
            foreach (var _referee in RobotsInArea.Values)
            {
                foreach (var _buff in BuffList)
                {
                    if (_buff.buffDuration >= 0.0f)
                    {
                        _referee.AddBuff(_buff);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        RefereeController robot = other.GetComponentInParent<RefereeController>();

        if (robot == null)
        {
            return;
        }

        if (belongFaction.Value != Faction.Neu && belongFaction.Value != robot.faction.Value)
        {
            return;
        }

        if (isControlPoint.Value && Occupied.Value && controllingFaction.Value != robot.faction.Value)
        {
            return;
        }

        controllingFaction.Value = robot.faction.Value;

        if (!RobotsInArea.ContainsKey(robot.RobotID.Value))
        {
            // Debug.Log("[BuffPoint] robot: " + robot.faction + robot.robotClass + robot.robotID);
            RobotsInArea.Add(robot.RobotID.Value, robot);
            // Debug.Log("[BuffPoint] robot counts: " + robotsInPoint.Count);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        RefereeController robot = other.GetComponentInParent<RefereeController>();

        if (robot == null)
        {
            return;
        }

        if (RobotsInArea.ContainsKey(robot.RobotID.Value))
        {
            RobotsInArea.Remove(robot.RobotID.Value);
        }
    }

}
