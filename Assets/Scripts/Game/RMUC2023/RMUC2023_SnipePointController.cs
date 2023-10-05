using Unity.Netcode;
using UnityEngine;

public class RMUC2023_SnipePointController : AreaController
{
    [SerializeField] private BuffEffectSO HeroSnipeBuff;
    [SerializeField] private NetworkVariable<Faction> faction = new NetworkVariable<Faction>(Faction.Neu); 

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
                if(_referee.robotClass.Value == RobotClass.Hero & _referee.faction.Value == faction.Value & _referee.faction.Value == controllingFaction.Value) _referee.AddBuff(HeroSnipeBuff);
            }
        }
    }
}