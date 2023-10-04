using UnityEngine;

public class RMUC2023_OutPostController : RefereeController 
{
    [SerializeField] private HingeJoint SpinJoint;

    public void StopSpin()
    {
        base.ShieldOff();
    }
}