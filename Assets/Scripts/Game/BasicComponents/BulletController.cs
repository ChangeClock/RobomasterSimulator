using UnityEngine;
using Unity.Netcode;

public class BulletController : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<int> attackerID = new NetworkVariable<int>(0);

    public float lastTime = 0.0f;
    public float liveTime = 10.0f;

    void FixedUpdate()
    {
        lastTime += Time.deltaTime;
        if (lastTime >= liveTime & GetComponent<Rigidbody>().velocity.magnitude == 0.0f) Destroy(this.gameObject); 
    }
}
