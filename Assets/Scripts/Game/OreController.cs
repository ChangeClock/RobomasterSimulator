using UnityEngine;
using Unity.Netcode;

public class OreController : NetworkBehaviour 
{
    public NetworkVariable<int> Value  = new NetworkVariable<int>(75);
    public NetworkVariable<OreType> Type  = new NetworkVariable<OreType>(OreType.Silver);

}