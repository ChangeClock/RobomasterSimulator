using Unity.Netcode;
using UnityEngine;

public class RMUC2023_BoostPoint : AreaController 
{
    [SerializeField] private BuffEffectSO PreBoostBuff;
    [SerializeField] private BuffEffectSO BoostBuff;

    void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (!Enabled.Value) return;

        // if (printLog) Debug.Log($"[AreaController] {ID} Area contains {RobotsInArea.Count} robots");

        if (RobotsInArea.Count > 0)
        {
            foreach (var _referee in RobotsInArea.Values)
            {
                if(_referee.HasBuff(PreBoostBuff)) _referee.AddBuff(BoostBuff);
            }
        }
    }    
}