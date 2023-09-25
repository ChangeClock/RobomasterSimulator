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
                Destroy(_UnitPointInfo.point);
                overtimePoints.Add(_UnitPointInfo.ID);
            }
        }
        
        if (overtimePoints.Count <= 0) return;

        foreach (var _id in overtimePoints)
        {
            UnitPoints.Remove(_id);
        }
    }

    public void SetPoint(int id, Vector2 location, float direction)
    {
        if (UnitPoints.ContainsKey(id))
        {
            UnitPoints[id].resetLastTime();
            UnitPoints[id].point.transform.position = new Vector3(location.x, location.y, 0);
            UnitPoints[id].point.GetComponent<UnitPointController>().SetDirection(direction);
        } else {
            GameObject _point = Instantiate(UnitPoint, new Vector3(location.x, location.y, 0), Quaternion.identity);
            _point.GetComponent<UnitPointController>().SetID(id);
            UnitPoints.Add(id, new UnitPointInfo(_point, id));
        }
    }
}