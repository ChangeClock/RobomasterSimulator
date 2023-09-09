using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RmGameplay/Robot Performance")]
public class RobotPerformanceSO : ScriptableObject
{
    public float[] maxHealth;
    public float[] maxLinearVelocity;
    public float[] maxAngularVelocity;
    public float[] maxHeat;
    public float[] cooldown;
    public float[] projectileVelocity;
}
