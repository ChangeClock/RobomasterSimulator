using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    // 为了使轮子的物理效果更加真实，有以下的改进项

    // A boolean property to store whether the gameobject is currently colliding with anything.
    private bool isColliding = false;
    private int isCollidingCounter = 10;

    private void OnCollisionStay(Collision collision) 
    {
        // Debug.Log("Tag: " + collision.gameObject.tag);

        if (collision.gameObject.tag == "Ground") 
        {
            isColliding = true;
            isCollidingCounter = 10;
        }
    }

    private void Update() {
        if (isCollidingCounter > 0)
        {
            isCollidingCounter --;
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
