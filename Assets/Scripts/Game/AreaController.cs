using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AeraController : MonoBehaviour
{
    [SerializeField]private int ID;
    // 0: not occupied 1: Blue 2: Red
    [SerializeField]private int Occupied;
    [SerializeField]private bool Enabled;

    public void SetID(int id)
    {
        ID = id;
    }

    public int GetID()
    {
        return ID;
    }

    public void SetOccupied(int occupied)
    {
        Occupied = occupied;
    }

    public int GetOccupied()
    {
        return Occupied;
    }

    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
    }

    public bool GetEnabled()
    {
        return Enabled;
    }
}
