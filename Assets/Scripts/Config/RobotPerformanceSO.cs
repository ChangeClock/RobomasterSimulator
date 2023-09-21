using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RmGameplay/Robot Performance")]
public class RobotPerformanceSO : ScriptableObject
{
    public float[] maxHealth;
    public int[] maxPower;
    public int[] maxHeat;
    public int[] coolDown;
    public int[] shootSpeed;
}
