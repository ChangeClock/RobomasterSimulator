using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RFIDController : MonoBehaviour
{
    public delegate void DetectAction(int areaID);
    public event DetectAction OnDetect;

    private bool isColliding = false;
    private int isCollidingCounter = 10;

    // 0: 中立区、无意义 
    private int areaID = 0;

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("[RFIDController] OnTriggerStay");
        if (other.tag == "Area")
        {
            AeraController _area = other.gameObject.GetComponent<AeraController>();
            if (!_area.GetEnabled()) return;

            isColliding = true;
            isCollidingCounter = 10;
            areaID = _area.GetID();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("[RFIDController] OnCollisionStay");
        if (collision.gameObject.tag == "Area")
        {
            AeraController _area = collision.gameObject.GetComponent<AeraController>();
            if (!_area.GetEnabled()) return;

            isColliding = true;
            isCollidingCounter = 10;
            areaID = _area.GetID();
        }
    }

    private void Update() 
    {
        if (isCollidingCounter > 0)
        {
            isCollidingCounter --;
        } else {
            isColliding = false;
        }

        if (isColliding)
        {
            Debug.Log($"[RFIDController] RFID detects area {areaID}");
            OnDetect(areaID);
        }
    }
}
