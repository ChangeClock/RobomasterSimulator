using UnityEngine;

public class RMUC2023_BaseController : RefereeController 
{
    public override void ShieldOff()
    {
        base.ShieldOff();
    }

    public override void Reset()
    {
        base.Reset();

        Immutable.Value = true;
    }
}