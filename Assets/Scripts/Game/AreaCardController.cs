using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AreaCardController : NetworkBehaviour
{
    [SerializeField]public NetworkVariable<int> ID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // 0: not occupied 1: Blue 2: Red
    [SerializeField]public NetworkVariable<bool> Occupied = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]public NetworkVariable<bool> Enabled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
}
