using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RmGameplay/ExchangePriceSO")]
public class ExchangePriceSO : ScriptableObject
{
    // Seconds taken per point
    public int[] silverPrice;
    public int[] goldPrice;
    public int[] levelLimit;
    public float[] levelBonus;
}
