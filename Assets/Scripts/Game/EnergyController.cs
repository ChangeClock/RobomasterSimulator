using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyController : NetworkBehaviour 
{
    [SerializeField] private WheelController[] wheels;

    [SerializeField] private float Power = 0.0f;
    [SerializeField] private float MaxPower = 200.0f;
    [SerializeField] private float Buffer = 60.0f;
    [SerializeField] private float MaxBuffer = 60.0f;
    [SerializeField] private float Energy = 0.0f;
    [SerializeField] private float MaxEnergy = 2000.0f;

    void Start()
    {
        foreach (WheelController wheel in wheels)
        {
            wheel.SetPowerLimit(MaxPower / wheels.Length);
        }
    }
    
    void Update()
    {

    }

    void FixedUpdate()
    {
        
    }

    public void SetMaxPower(float var)
    {
        MaxPower = var;
        
        foreach (WheelController wheel in wheels)
        {
            wheel.SetPowerLimit(MaxPower / wheels.Length);
        }
    }

    public float GetPower()
    {
        return Power;
    }
    
    public bool IsOverPower()
    {
        return (Mathf.Abs(Power) > MaxPower);
    }

    public void SetMaxBuffer(float var)
    {
        MaxBuffer = var;
    }

    public float GetMaxBuffer()
    {
        return MaxBuffer;
    }

    public float GetBuffer()
    {
        return Buffer;
    }

    public void RefillBuffer()
    {
        Buffer = MaxBuffer;
    }

}