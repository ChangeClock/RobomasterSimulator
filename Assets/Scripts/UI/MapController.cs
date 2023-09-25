using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{
    [SerializeField] private GameObject UnitPoint;

    private Dictionary<int, UnitPointController> UnitPoints = new Dictionary<int, UnitPointController>();

    public void SetPoint(bool enable, int id, Vector2 location)
    {
        if (enable) 
        {
        }
    }
}