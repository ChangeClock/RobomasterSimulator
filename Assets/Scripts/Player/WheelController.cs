using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    // 为了使轮子的物理效果更加真实，有以下的改进项

    // A boolean property to store whether the gameobject is currently colliding with anything.
    private bool isColliding = false;

    // // OnTriggerEnter is called when this gameobject first enters a collision with another object.
    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.tag == "Ground") 
    //     {
    //         isColliding = true;
    //     }
    // }

    // // OnTriggerExit is called when this gameobject exits a collision with another object.
    // private void OnCollisionExit(Collision collision)
    // {
    //     if (collision.gameObject.tag == "Ground") 
    //     {
    //         isColliding = false;
    //     }
    // }

    private void OnCollisionStay(Collision collision) 
    {
        if (collision.gameObject.tag == "Ground") 
        {
            isColliding = true;
        } else {
            isColliding = false;
        }
    }

    // You can call this function externally if you need to know whether this object is currently colliding with something.
    public bool IsColliding()
    {
        return isColliding;
    }

    // TODO：检测压力情况，模拟摩擦力大小变化，影响加力大小/上限


    // TODO: 输出接触面切向方向，优化目前在底盘水平面上加力的问题
}
