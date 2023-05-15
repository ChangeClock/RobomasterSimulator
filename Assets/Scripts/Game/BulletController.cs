using UnityEngine;


public class BulletController : MonoBehaviour
{
    // A bullet spawn with a speed of 10 and will bounce whenever it hits something, and will be affected by gravity and air resistance
    public float speed = 0f;
    public float lifetime = 60f;

    // setter and getter of current position
    public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        Invoke("Deactivate", lifetime); // Deactivate the bullet after its lifetime has passed
    }

    void Update()
    {
        Debug.Log("Bullet position: " + transform.position);
        rb.velocity = transform.forward * speed; // Move the bullet forward
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Bullet")) // Ignore collisions with players and bullets
            return;
            
        Deactivate();
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
