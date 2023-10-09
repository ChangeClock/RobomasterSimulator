using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AreaController : NetworkBehaviour
{
    public NetworkVariable<int> ID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // 0: not occupied 1: Blue 2: Red
    public NetworkVariable<bool> Enabled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public bool printLog = false;

    public NetworkVariable<Faction> belongFaction = new NetworkVariable<Faction>(Faction.Neu);
    public NetworkVariable<bool> isControlPoint = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> Occupied = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<Faction> controllingFaction = new NetworkVariable<Faction>(Faction.Neu);

    public NetworkVariable<float> MaxCaptureProgress = new NetworkVariable<float>(100.0f);
    public NetworkVariable<float> CaptureProgress = new NetworkVariable<float>(0.0f);
    public NetworkVariable<float> CaptureProgressPerSecond = new NetworkVariable<float>(1.0f);
    
    public NetworkVariable<float> MaxResetProgress = new NetworkVariable<float>(0.0f);
    public NetworkVariable<float> ResetProgress = new NetworkVariable<float>(0.0f);
    public NetworkVariable<float> ResetProgressPerSecond = new NetworkVariable<float>(1.0f);

    public List<RobotTag> TagList = new List<RobotTag>();

    [SerializeField] private BuffEffectSO[] BuffList;

    public delegate void CaptureAction();
    public event CaptureAction OnCaptured;

    protected Dictionary<int, RefereeController> RobotsInArea = new Dictionary<int, RefereeController>();

    protected virtual void Start()
    {
        if (!IsServer) return;

        // gameObject.GetComponent<NetworkObject>().Spawn();
    }

    protected virtual void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        if (!Enabled.Value) return;

        // if (printLog) Debug.Log($"[AreaController] {ID} Area contains {RobotsInArea.Count} robots");

        if (RobotsInArea.Count > 0)
        {
            bool isControlled = false;

            foreach (var _referee in RobotsInArea.Values)
            {
                // Gain control when the area is not under control
                if (controllingFaction.Value == Faction.Neu) controllingFaction.Value = _referee.faction.Value;

                if (isControlPoint.Value & controllingFaction.Value != _referee.faction.Value) continue;

                foreach (var _buff in BuffList)
                {
                    if (_buff.buffDuration >= 0.0f)
                    {
                        _referee.AddBuff(_buff);
                    }
                }

                isControlled = true;
            }

            Occupied.Value = isControlled;

            // When there's no more orignal control faction robot in area, lose control
        } else {
            Occupied.Value = false;
        }

        if (ResetProgress.Value >= MaxResetProgress.Value)
        {
            if (Occupied.Value)
            {
                if (CaptureProgress.Value >= MaxCaptureProgress.Value) 
                {
                    Capture();
                } else {
                    CaptureProgress.Value += CaptureProgressPerSecond.Value * Time.deltaTime;
                }
            } else {
                CaptureProgress.Value = 0;
                controllingFaction.Value = Faction.Neu;
            }
        } else {
            ResetProgress.Value += ResetProgressPerSecond.Value * Time.deltaTime;
        }


        // if (printLog) Debug.Log($"[AreaController] Capture Progress {CaptureProgress.Value}/{MaxCaptureProgress.Value}");
    }

    protected virtual void OnTriggerEnter(Collider other)
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

        if (TagList.Count > 0)
        {
            bool allowed = false;
            foreach (var tag in TagList)
            {
                if (robot.robotTags.Contains(tag)) allowed = true;
            }

            if (!allowed) return;
        }

        if (belongFaction.Value != Faction.Neu && belongFaction.Value != robot.faction.Value)
        {
            return;
        }

        if (isControlPoint.Value && !Occupied.Value)
        {
            controllingFaction.Value = robot.faction.Value;
        }

        if (!RobotsInArea.ContainsKey(robot.RobotID.Value))
        {
            // Debug.Log("[BuffPoint] robot: " + robot.faction + robot.robotClass + robot.robotID);
            RobotsInArea.Add(robot.RobotID.Value, robot);
            // Debug.Log("[BuffPoint] robot counts: " + robotsInPoint.Count);
        }

    }

    protected virtual void OnTriggerExit(Collider other)
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

    public bool ContainsRobot(int robotID)
    {
        return RobotsInArea.ContainsKey(robotID);
    }

    public void ResetCaptureProgress()
    {
        ResetProgress.Value = 0;
        CaptureProgress.Value = 0;
    }

    protected virtual void Capture()
    {

    }

}
