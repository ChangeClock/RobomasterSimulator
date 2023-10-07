using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    // 为了使轮子的物理效果更加真实，有以下的改进项

    // A boolean property to store whether the gameobject is currently colliding with anything.
    private bool isColliding = false;
    private int isCollidingCounter = 10;

    private Vector3 lastPos;

    [SerializeField] private float power = 0.0f;
    [SerializeField] private float powerLimit = 60.0f;
    [SerializeField] private float torque = 800.0f;

    public bool printLog = false;

    [SerializeField] private Vector3 wheelForceDirection = new Vector3(1,0,1);

    private void OnCollisionStay(Collision collision) 
    {
        // Debug.Log("Tag: " + collision.gameObject.tag);

        if (collision.gameObject.tag == "Ground") 
        {
            isColliding = true;
            isCollidingCounter = 10;
        }
    }

    void Start()
    {
        lastPos = transform.position;
    }

    private void Update() 
    {
        if (isCollidingCounter > 0)
        {
            isCollidingCounter --;
        } else {
            isColliding = false;
        }
    }

    private void FixedUpdate()
    {
        if (power > powerLimit) power = powerLimit;

        float displacement = 100 * Vector3.Distance(transform.position, lastPos) + 1.0f;
        float wheelForce = power / displacement;
        lastPos = transform.position;

        if (printLog) Debug.Log($"power {displacement * wheelForce}");

        Vector3 direction = wheelForceDirection.x * transform.forward + wheelForceDirection.y * transform.up - wheelForceDirection.z * transform.right;

        // if (printLog) Debug.Log($"Thruster {thruster}, Displacement {displacement}, WheelForce {wheelForce}");

        gameObject.GetComponent<Rigidbody>().AddForce(wheelForce * direction * (isColliding ? 1 : 0) * torque);
        Debug.DrawLine(transform.position, transform.position + (direction * wheelForce * (isColliding ? 1 : 0) * 25f) , Color.red);
    }

    // You can call this function externally if you need to know whether this object is currently colliding with something.
    public bool IsColliding()
    {
        return isColliding;
    }

    public void SetPower(float var)
    {
        power = var;
    }
    
    public float GetPower()
    {
        return power;
    }

    // TODO：检测压力情况，模拟摩擦力大小变化，影响加力大小/上限


    // TODO: 输出接触面切向方向，优化目前在底盘水平面上加力的问题
}
