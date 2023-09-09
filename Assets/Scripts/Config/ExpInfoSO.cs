using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RmGameplay/ExpInfo")]
public class ExpInfoSO : ScriptableObject
{
    // Seconds taken per point
    public float expGrowth;
    public float[] expToNextLevel;
    public float[] expValue;
}
