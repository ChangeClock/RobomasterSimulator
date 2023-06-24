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

    public int ArmorID;

    public bool Enabled;
    public int LightColor;

    // damageType: 0 碰撞; 1 17mm; 2 42mm; 3 导弹;
    public delegate void HitAction(int damageType, int armorID);
    public event HitAction OnHit;

    // damageDetection: 对应上面的damageType，设定此装甲板是否响应对应的伤害
    public bool[] damageDetection = {true,true,true,false};

    private LightController armorLight;
    private Color purple = new Color(0.57f,0.25f,1f,1f);

    void Start() 
    {
        Transform light = transform.Find("Light");
        if (light != null){
            armorLight = light.GetComponent<LightController>();
        }
    }

    void Update()
    {
        if (armorLight != null){
            armorLight.Enabled = Enabled;
            armorLight.LightColor = LightColor;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Calculate the angle of impact between the collider and the armor
        Vector3 incomingVelocity = collision.relativeVelocity;
        Vector3 normal = collision.contacts[0].normal;
        Vector3 perpendicularVelocity = Vector3.ProjectOnPlane(incomingVelocity, normal);

        // Debug.Log("[ArmorController] Armor on Hit wth velocity:" + perpendicularVelocity);
        if (!Enabled) return;

        if (collision.gameObject.tag == "Bullet-17mm" && damageDetection[1]) {
            // Check if the final velocity is above the minimum required
            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThreshold17mm) >= 0f)
            {
                // Debug.Log("On Hit with 17mm");
                if (OnHit != null) OnHit(1,ArmorID);
                StartCoroutine(Blink());
            }

        } else if (collision.gameObject.tag == "Bullet-42mm" & damageDetection[2]) {

            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThreshold42mm) >= 0f)
            {
                // Debug.Log("On Hit with 42mm");
                if (OnHit != null) OnHit(2,ArmorID);
                StartCoroutine(Blink());
            }

        } else if (collision.gameObject.tag == "Missle" & damageDetection[3]) {

            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThresholdMissle) >= 0f)
            {
                // Debug.Log("On Hit with Missle");
                if(OnHit != null) OnHit(0,ArmorID);
                StartCoroutine(Blink());
            }

        } else if (damageDetection[0]) {

            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThresholdImpact) >= 0f)
            {
                // Debug.Log("On Hit with Impact");
                if(OnHit != null) OnHit(0,ArmorID);
                StartCoroutine(Blink());
            }

        }

        // Debug.Log("Bullet hit armor" + perpendicularVelocity + " " + perpendicularVelocity.magnitude);
    }

    IEnumerator Blink()
    {
        // Debug.Log("Light Off");
        Enabled = false;
        yield return new WaitForSeconds(0.1f);
        Enabled = true;
        // Debug.Log("Light On");
    }
}
