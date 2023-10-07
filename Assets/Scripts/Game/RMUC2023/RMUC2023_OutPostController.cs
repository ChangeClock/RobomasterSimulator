using UnityEngine;
using Unity.Netcode;

public class RMUC2023_OutPostController : RefereeController 
{
    [SerializeField] private HingeJoint SpinJoint;
    [SerializeField] private float TargetVelocity = 144; // 114 degree/s = 0.4 r/s
    [SerializeField] private int SpinDirection = 1;
    [SerializeField] private Quaternion InitialRotation;
    [SerializeField] public NetworkVariable<bool> Suppressed = new NetworkVariable<bool>(false);
    [SerializeField] public NetworkVariable<bool> Stopped = new NetworkVariable<bool>(false);

    [SerializeField] public bool HasGivenEXP = false;

    protected override void Start()
    {
        InitialRotation = SpinJoint.transform.localRotation;

        if (Random.Range(-1,1) < 0)
        {
            SpinDirection = -1;
        } else {
            SpinDirection = 1;
        }
    }

    protected override void Update()
    {
        base.Update();

        var motor = SpinJoint.motor;

        if (Stopped.Value || !Enabled.Value)
        {
            motor.targetVelocity = 0;
        } else if (Suppressed.Value) {
            motor.targetVelocity = SpinDirection * TargetVelocity / 2;
        } else {
            motor.targetVelocity = SpinDirection * TargetVelocity;
        }

        SpinJoint.motor = motor;
    }

    public void Stop()
    {
        Stopped.Value = true;
        SpinJoint.transform.localRotation = InitialRotation;
    }

    public override void Reset()
    {
        base.Reset();
        
        Suppressed.Value = false;
        Stopped.Value = false;

        if (Random.Range(-1,1) < 0)
        {
            SpinDirection = -1;
        } else {
            SpinDirection = 1;
        }
    }
}