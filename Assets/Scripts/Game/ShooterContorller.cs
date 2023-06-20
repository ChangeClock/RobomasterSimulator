using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;



public class ShooterController : MonoBehaviour
{
    // Only upload the shooter postion, velocity and ShooterID, all the other details like whether this shootaction is valid will be judged by refereecontroller
    // This is called trigger action is because the player only pulls the trigger, whether there will be a bullet is determined by other factors...
    public delegate void TriggerAction(int shooterID, Vector3 userPosition, Vector3 shootVelocity);
    public event TriggerAction OnTrigger;

    [SerializeField]private int ShooterID;

    // status
    private int HeatLimit;
    private int Heat;
    
    private bool Enabled;

    void Update()
    {
        // Update light effects on shooter components according to head and heatlimit
    }

    public void PullTrigger(float Speed)
    {
        if (!Enabled) return;

        // Debug.Log("[ShooterController] PullTrigger");
        OnTrigger(ShooterID, this.gameObject.transform.position, this.gameObject.transform.right * Speed);
    }

    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
    }

    public bool GetEnabled(bool enabled)
    {
        return Enabled;
    }
}