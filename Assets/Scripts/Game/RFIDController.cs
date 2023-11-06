using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RFIDController : MonoBehaviour
{
    public delegate void DetectAction(AreaController area);
    public event DetectAction OnDetect;

    private bool isColliding = false;
    private int isCollidingCounter = 10;

    // 0: 中立区、无意义 
    private AreaController area;

    private void OnTriggerStay(Collider other)
    {
        // Debug.Log($"[RFIDController] OnTriggerStay {other.tag}");
        if (other.tag == "Area")
        {
            area = other.gameObject.GetComponent<AreaController>();
            // Debug.Log($"[RFIDController] {other.gameObject}");
            if (area == null) return;
            if (!area.Enabled.Value) return;

            isColliding = true;
            isCollidingCounter = 10;
        }
    }

    private void Update() 
    {
        if (isCollidingCounter > 0)
        {
            isCollidingCounter --;
        } else {
            isColliding = false;
            area = null;
        }

        if (isColliding)
        {
            // Debug.Log($"[RFIDController] RFID detects area {area.ID.Value}");
            OnDetect(area);
        }
    }
}
