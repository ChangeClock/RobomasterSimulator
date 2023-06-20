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
    private List<List<GameObject>> bulletsList = new List<List<GameObject>>();

    void Start()
    {
        bulletsList.Add(bullets_17mm);
        bulletsList.Add(bullets_42mm);

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
        
        GameObject.Destroy(_bullet);
    }

    void OnEnable()
    {
        RefereeController.OnShoot += Shoot;
    }

    void OnDisable()
    {
        RefereeController.OnShoot -= Shoot;
    }

    void Shoot(int shooterID, int shooterType, int robotID, Vector3 userPosition, Vector3 shootVelocity)
    {
        // 封装过多，需要改

        // if (shooterType == 0){
        //     bulletsList = & bullets_17mm;
        // } else if (shooterType == 1) {
        //     bulletsList = & bullets_42mm;
        // } else {
        //     Debug.LogError("Unknown Shooter Type");
        // }

        Quaternion userDirection = Quaternion.Euler(shootVelocity.x, shootVelocity.y, shootVelocity.z);
        foreach (GameObject bullet in bulletsList[shooterType])
        {
            if (!bullet.activeInHierarchy)
            {
                bullet.transform.position = userPosition;
                bullet.transform.rotation = userDirection;
                bullet.GetComponent<Rigidbody>().velocity = shootVelocity;
                bullet.SetActive(true);
                return;
            }

            // TODO: Reuse the oldest bullets
        }

        // Debug.Log($"Shoot {userPosition}, {userDirection}, {ShootVelocity}");
    }
}