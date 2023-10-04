using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RmGameplay/BuffEffect")]
public class BuffEffectSO : ScriptableObject
{
    // All float number communicate through integer type to precision loss.
    public int buffDuration = 0; // s
    public int DEFBuff = 0; // %
    public int ATKBuff = 0; // %
    public int CDBuff = 0; // magnification
    public int ReviveProgressPerSec = 0; // %
    public int HealBuff = 0; // %
}
