using UnityEngine;

public class UWBController : MonoBehaviour 
{
    public Vector2 Position;
    public float Direction;

    void Update()
    {
        Position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        Direction = gameObject.transform.eulerAngles.y;
    }     
}