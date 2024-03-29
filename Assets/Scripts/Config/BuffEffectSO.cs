﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RmGameplay/BuffEffect")]
public class BuffEffectSO : ScriptableObject
{
    // All float number communicate through integer type to precision loss.
    public int buffDuration = 0; // ms
    public int DEFBuff = 0; // %
    public int DEFDeBuff = 0; // %
    public int ATKBuff = 0; // %
    public int CDBuff = 0; // magnitude
    public int ReviveProgressPerSec = 0; // %
    public int HealBuff = 0; // %
    public int InSupplyArea = 0; // bool
    public int IsActivatingBuff = 0;
    public int IsMining = 0;

    public BuffEffectSO(int duration, int def, int atk)
    {
        buffDuration = duration;
        DEFBuff = def;
        ATKBuff = atk;
    }
}