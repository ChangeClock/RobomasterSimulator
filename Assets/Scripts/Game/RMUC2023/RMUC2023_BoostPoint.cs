using Unity.Netcode;
using UnityEngine;

public class RMUC2023_BoostPoint : AreaController 
{
    [SerializeField] private BuffEffectSO PreBoostBuff;

    protected override void FixedUpdate()
    {
        if (!IsServer) return;

        if (!Enabled.Value) return;

        if (RobotsInArea.Count > 0)
        {
            foreach (var _referee in RobotsInArea.Values)
            {
                if (_referee.HasBuff(PreBoostBuff))
                {
                    foreach (var _buff in BuffList)
                    {
                        if (_buff.buffDuration >= 0.0f)
                        {
                            _referee.AddBuff(_buff);
                        }
                    }
                    _referee.RemoveBuff(PreBoostBuff);
                }
            }
        }
    }
}