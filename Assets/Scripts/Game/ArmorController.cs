using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ArmorController : MonoBehaviour
{
    public float velocityThresholdImpact = 6f;
    public float velocityThreshold17mm = 12f;
    public float velocityThreshold42mm = 8f;
    public float velocityThresholdMissle = 6f;

    public int armorID;

    // damageType: 0 碰撞; 1 17mm; 2 42mm; 3 导弹;
    public delegate void HitAction(int damageType, int armorID);
    public event HitAction OnHit;

    // damageDetection: 对应上面的damageType，设定此装甲板是否响应对应的伤害
    public bool[] damageDetection = {true,true,true,false};

    void OnCollisionEnter(Collision collision)
    {
        // Calculate the angle of impact between the collider and the armor
        Vector3 incomingVelocity = collision.relativeVelocity;
        Vector3 normal = collision.contacts[0].normal;
        Vector3 perpendicularVelocity = Vector3.ProjectOnPlane(incomingVelocity, normal);

        if (collision.gameObject.tag == "Bullet-17mm" && damageDetection[1])
        {
            // Check if the final velocity is above the minimum required
            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThreshold17mm) >= 0f && OnHit != null)
            {
                OnHit(1,armorID);
                // Debug.Log("OnHit");
            }
            
        } else if (collision.gameObject.tag == "Bullet-42mm" & damageDetection[2]) {
            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThreshold42mm) >= 0f && OnHit != null)
            {
                OnHit(2,armorID);
                // Debug.Log("OnHit");
            }
        } else if (collision.gameObject.tag == "Missle" & damageDetection[3]) {

            // TODO: 我特么射爆

        } else if (damageDetection[0]) {
            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThresholdImpact) >= 0f && OnHit != null)
            {
                OnHit(0,armorID);
                // Debug.Log("OnHit");
            }
        }

        // Debug.Log("Bullet hit armor" + perpendicularVelocity + " " + perpendicularVelocity.magnitude);
    }
}
