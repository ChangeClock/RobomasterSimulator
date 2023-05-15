using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class BulletManager : MonoBehaviour
{
    [SerializeField] private GameObject Bullet_17mm;
    [SerializeField] private GameObject Bullet_42mm;
    public int maxBullets;
    
    private List<GameObject> bullets_17mm = new List<GameObject>();
    private List<GameObject> bullets_42mm = new List<GameObject>();

    void Start()
    {
        GameObject _bullet = new GameObject();
        for (int i = 0; i < maxBullets; i++)
        {
            _bullet = Instantiate(Bullet_17mm);
            _bullet.SetActive(false);
            bullets_17mm.Add(_bullet);
            _bullet = Instantiate(Bullet_42mm);
            _bullet.SetActive(false);
            bullets_42mm.Add(_bullet);
        }
    }

    public void GetBullet17mm(Vector3 userPosition, Quaternion userDirection, Vector3 ShootVelocity)
    {
        foreach (GameObject bullet in bullets_17mm)
        {
            if (!bullet.activeInHierarchy)
            {
                bullet.transform.position = userPosition;
                bullet.transform.rotation = userDirection;
                bullet.GetComponent<Rigidbody>().velocity = ShootVelocity;
                bullet.SetActive(true);
                return;
            }
        }

        // If all bullets_17mm are in use, return null or create a new one if needed
        GameObject newBullet = Instantiate(Bullet_17mm, userPosition, userDirection);
        newBullet.GetComponent<Rigidbody>().velocity = ShootVelocity;
        bullets_17mm.Add(newBullet);
        return;
    }

    public void GetBullet42mm(Vector3 userPosition, Quaternion userDirection, Vector3 ShootVelocity)
    {
        foreach (GameObject bullet in bullets_42mm)
        {
            if (!bullet.activeInHierarchy)
            {
                bullet.transform.position = userPosition;
                bullet.transform.rotation = userDirection;
                bullet.GetComponent<Rigidbody>().velocity = ShootVelocity;
                bullet.SetActive(true);
                return;
            }
        }

        // If all bullets_17mm are in use, return null or create a new one if needed
        GameObject newBullet = Instantiate(Bullet_17mm, userPosition, userDirection);
        newBullet.GetComponent<Rigidbody>().velocity = ShootVelocity;
        bullets_17mm.Add(newBullet);
        return;
    }

    void OnEnable()
    {
        RobotController.OnShoot += Shoot;
    }

    void OnDisable()
    {
        RobotController.OnShoot -= Shoot;
    }

    void Shoot(Vector3 userPosition, Quaternion userDirection, Vector3 ShootVelocity, int ShooterType)
    {
        // 封装过多，需要改

        if (ShooterType == 0){
            GetBullet17mm(userPosition, userDirection, ShootVelocity);
        } else if (ShooterType == 1) {
            GetBullet42mm(userPosition, userDirection, ShootVelocity);
        } else {
            Debug.LogError("Unknown Shooter Type");
        }
        // Debug.Log($"Shoot {userPosition}, {userDirection}, {ShootVelocity}");
    }
}