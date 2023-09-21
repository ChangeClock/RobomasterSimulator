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
        // Debug.Log($"[RFIDController] OnTriggerStay {other.tag}");
        if (other.tag == "Area")
        {
            AreaCardController _areaCard = other.gameObject.GetComponent<AreaCardController>();
            // Debug.Log($"[RFIDController] {other.gameObject}");
            if (_areaCard == null) return;
            if (!_areaCard.Enabled.Value) return;

            isColliding = true;
            isCollidingCounter = 10;
            areaID = _areaCard.ID.Value;
        }
    }

    private void Update() 
    {
        if (isCollidingCounter > 0)
        {
            isCollidingCounter --;
        } else {
            isColliding = false;
            areaID = 0;
        }

        if (isColliding)
        {
            Debug.Log($"[RFIDController] RFID detects area {areaID}");
            OnDetect(areaID);
        }
    }
}
