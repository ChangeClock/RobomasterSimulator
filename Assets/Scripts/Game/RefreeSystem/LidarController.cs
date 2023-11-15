using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LidarController : RefereeController
{
    private List<Camera> CameraList = new List<Camera>();
    [SerializeField] private float MarkFrequency = 5f;
    [SerializeField] private float _markTimeout = 0f;

    protected override void Start()
    {
        base.Start();

        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (var camera in cameras)
        {
            CameraList.Add(camera);
        }
    }

    private bool IsVisible(Camera c, GameObject target)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(c);
        var point = target.transform.position;

        foreach (var plane in planes)
        {
            if (plane.GetDistanceToPoint(point)< 0)
            {
                return false;
            }
        }

        // Check if the target is blocked by other objects
        RaycastHit hit;

        if (Physics.Linecast(c.transform.position, point, out hit))
        {
            // check it the first object in the line is the target or its child
            if (hit.transform.gameObject == target || hit.transform.IsChildOf(target.transform))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    protected override void Update()
    {
        if (!IsServer) return;

        if (_markTimeout < 1 / MarkFrequency)
        {
            _markTimeout += Time.deltaTime;
            return;
        }

        foreach (var robot in gameManager.RefereeControllerList.Values)
        {
            if (robot == null) continue;
            if (!robot.Enabled.Value) continue;
            if (robot.faction.Value == faction.Value) continue;

            if (robot.robotTags.Contains(RobotTag.Building)) continue;

            foreach (var camera in CameraList)
            {
                if (IsVisible(camera, robot.gameObject))
                {
                    // Debug.Log($"[LidarController] robot {robot.RobotID.Value} is visible");
                    MarkRequestHandler(robot.RobotID.Value, robot.Position);
                }
            }
        }

        _markTimeout = 0f;
    }
}
