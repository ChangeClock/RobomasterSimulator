using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RMUC2023_MineArea : AreaController 
{
    [SerializeField] private GameObject Ore;
    public Stack<OreController> OreList = new Stack<OreController>();
    public List<Transform> OreStorePoints = new List<Transform>();

    protected override void Start()
    {
        base.Start();

        // if (printLog) Debug.Log($"[MineArea] OreStorePoints : {OreStorePoints.Count}");

        if (!IsServer) return;

        if (OreStorePoints.Count > 0)
        {
            foreach (var point in OreStorePoints)
            {
                GameObject _ore = Instantiate(Ore, point.position, point.rotation);
                _ore.GetComponent<NetworkObject>().Spawn();
                OreList.Push(_ore.GetComponent<OreController>());
                // Debug.Log($"[MineArea] Spawn Ore {OreList.Count}");
                // if (printLog) Debug.Log($"[MineArea] Spawn Ore:{_ore.GetComponent<NetworkObject>().NetworkObjectId}");
            }
        }
    }

    public OreController GetOre()
    {
        return OreList.Pop();
    }

    public void AddOre(int id)
    {

    }

    protected override void Capture()
    {
        base.Capture();

        Debug.Log($"[MineArea] Ore {OreList.Count}");

        if (OreList.Count > 0)
        {
            foreach (var referee in RobotsInArea.Values)
            {
                if (referee.faction.Value == controllingFaction.Value & referee.robotClass.Value == RobotClass.Engineer)
                {
                    ResetCaptureProgress();
                    referee.AddOre(GetOre());
                    return;
                }
            }

            ResetCaptureProgress();
            controllingFaction.Value = Faction.Neu;
        }
    }
}