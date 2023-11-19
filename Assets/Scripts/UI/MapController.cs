using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{
    [SerializeField] private GameObject UnitPoint;

    private class UnitPointInfo{
        public GameObject point;
        public int ID;
        public float lastTime;
        public float liveTime = 1.0f;
        
        public UnitPointInfo(GameObject var, int id)
        {
            ID = id;
            point = var;
            lastTime = liveTime;
        }

        public void resetLastTime()
        {
            lastTime = liveTime;
        }
    }

    private Dictionary<int, UnitPointInfo> UnitPoints = new Dictionary<int, UnitPointInfo>();

    void Update()
    {
        if (UnitPoints.Count <= 0) return;

        List<int> overtimePoints = new List<int>(); 

        foreach (var _UnitPointInfo in UnitPoints.Values)
        {
            _UnitPointInfo.lastTime -= Time.deltaTime;
            if (_UnitPointInfo.lastTime <= 0)
            {
                overtimePoints.Add(_UnitPointInfo.ID);
            }
        }
        
        if (overtimePoints.Count <= 0) return;

        foreach (var _id in overtimePoints)
        {
            // Destroy(UnitPoints[_id].point);
            UnitPoints.Remove(_id);
        }
    }

    public void SetPoint(int id, Faction faction, Vector2 location, float direction)
    {
        int displayID = id;
        if (faction == Faction.Blue) displayID -= 20;

        if (!UnitPoints.ContainsKey(id))
        {
            GameObject _point = Instantiate(UnitPoint, gameObject.transform);
            _point.GetComponent<UnitPointController>().SetID(displayID);
            UnitPoints.Add(id, new UnitPointInfo(_point, displayID));
        }
        
        UnitPoints[id].resetLastTime();
        // Debug.Log($"[MapController] Position {location}");
        UnitPoints[id].point.GetComponent<RectTransform>().anchoredPosition = location;
        // Debug.Log($"[MapController] Position {UnitPoints[id].point.transform.position}");
        UnitPoints[id].point.GetComponent<UnitPointController>().SetDirection(direction);

        switch (faction)
        {
            case Faction.Self:
                UnitPoints[id].point.GetComponent<UnitPointController>().SetColor(Color.green);
                break;
            case Faction.Red:
                UnitPoints[id].point.GetComponent<UnitPointController>().SetColor(Color.red);
                break;
            case Faction.Blue:
                UnitPoints[id].point.GetComponent<UnitPointController>().SetColor(Color.blue);
                break;
            default:
                break;
        }
    }

    public void SetPoint(int id, Faction faction, Vector2 location)
    {
        int displayID = id;
        if (faction == Faction.Blue) displayID -= 20;

        if (!UnitPoints.ContainsKey(id))
        {
            GameObject _point = Instantiate(UnitPoint, gameObject.transform);
            _point.GetComponent<UnitPointController>().SetID(displayID);
            UnitPoints.Add(id, new UnitPointInfo(_point, displayID));
        }
        
        UnitPoints[id].resetLastTime();
        // Debug.Log($"[MapController] Position {location}");
        UnitPoints[id].point.GetComponent<RectTransform>().anchoredPosition = location;
        // Debug.Log($"[MapController] Position {UnitPoints[id].point.transform.position}");
        // UnitPoints[id].point.GetComponent<UnitPointController>().SetDirection(direction);

        switch (faction)
        {
            case Faction.Self:
                UnitPoints[id].point.GetComponent<UnitPointController>().SetColor(Color.green);
                break;
            case Faction.Red:
                UnitPoints[id].point.GetComponent<UnitPointController>().SetColor(Color.red);
                break;
            case Faction.Blue:
                UnitPoints[id].point.GetComponent<UnitPointController>().SetColor(Color.blue);
                break;
            default:
                break;
        }
    }
}