using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RMUC2023_MineMountain : NetworkBehaviour 
{
    [SerializeField] private GameObject Ore;
    public Stack<OreController> OreList = new Stack<OreController>();
    public List<Transform> OreStorePoints = new List<Transform>();

    [SerializeField] List<AreaController> MiningAreas = new List<AreaController>();

    void OnEnable()
    {
        foreach (var area in MiningAreas)
        {
            area.OnCaptured += Mined;
        }
    }

    void Start()
    {
        if (!IsServer) return;

        ResetOre();
    }

    public void ResetOre()
    {
        if (!IsServer) return;

        while(OreList.Count > 0)
        {
            Destroy(OreList.Pop().gameObject);
        }

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

    void Mined(RefereeController robot)
    {
        // Debug.Log($"[MineArea] Ore {OreList.Count}");

        if (OreList.Count > 0)
        {
            robot.AddOre(GetOre());
        }
    }
}