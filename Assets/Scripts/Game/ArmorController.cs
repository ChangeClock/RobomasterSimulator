using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ArmorController : MonoBehaviour
{
    public int ArmorID;

    public bool Enabled;
    public bool IsBlink;
    public int LightColor;

    // damageType: 0 碰撞; 1 17mm; 2 42mm; 3 导弹;
    public delegate void HitAction(int damageType, float damage ,int armorID, int attackerID = 0);
    public event HitAction OnHit;

    public float[] velocityThreshold = {6f, 12f, 8f, 6f};
    public float[] damage = {2.0f, 10.0f, 100.0f, 0.0f};

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
            if (IsBlink) return;
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

        if (collision.gameObject.tag == "Bullet-17mm" && damage[1] > 0) {
            // Check if the final velocity is above the minimum required
            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThreshold[1]) >= 0f)
            {
                // Debug.Log("On Hit with 17mm");
                if (OnHit != null) OnHit(1, damage[1] ,ArmorID, collision.gameObject.GetComponent<BulletController>().attackerID.Value);
                StartCoroutine(Blink());
            }

        } else if (collision.gameObject.tag == "Bullet-42mm" & damage[2] > 0) {

            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThreshold[2]) >= 0f)
            {
                // Debug.Log("On Hit with 42mm");
                if (OnHit != null) OnHit(2, damage[2] ,ArmorID, collision.gameObject.GetComponent<BulletController>().attackerID.Value);
                StartCoroutine(Blink());
            }

        } else if (collision.gameObject.tag == "Missle" & damage[3] > 0) {

            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThreshold[3]) >= 0f)
            {
                // Debug.Log("On Hit with Missle");
                if(OnHit != null) OnHit(3, damage[3] ,ArmorID, collision.gameObject.GetComponent<BulletController>().attackerID.Value);
                StartCoroutine(Blink());
            }

        } else if (damage[0] > 0) {

            if (Mathf.Abs(perpendicularVelocity.magnitude - velocityThreshold[0]) >= 0f)
            {
                // Debug.Log("On Hit with Impact");
                if(OnHit != null) OnHit(0, damage[0] ,ArmorID);
                StartCoroutine(Blink());
            }

        }

        // Debug.Log("Bullet hit armor" + perpendicularVelocity + " " + perpendicularVelocity.magnitude);
    }

    IEnumerator Blink()
    {
        // Debug.Log("Light Off");
        IsBlink = true;
        Enabled = false;
        yield return new WaitForSeconds(0.1f);
        Enabled = true;
        IsBlink = false;
        // Debug.Log("Light On");
    }
}
