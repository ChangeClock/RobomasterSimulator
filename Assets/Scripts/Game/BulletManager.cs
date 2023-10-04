using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using Unity.Netcode;

public class BulletManager : NetworkBehaviour
{
    [SerializeField] private GameObject Bullet_17mm;
    [SerializeField] private GameObject Bullet_42mm;

    void Start()
    {

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
        Quaternion userDirection = Quaternion.Euler(shootVelocity.x, shootVelocity.y, shootVelocity.z);

        GameObject _bullet;

        switch (shooterType)
        {
            case 0:
                _bullet = Instantiate(Bullet_17mm, userPosition, userDirection);
                break;
            case 1:
                _bullet = Instantiate(Bullet_42mm, userPosition, userDirection);
                break;
            default:
                Debug.LogError($"[BulletManager] Unknown shooterType {shooterType}!");
                return;
        }

        _bullet.GetComponent<Rigidbody>().isKinematic = false;
        _bullet.GetComponent<Rigidbody>().velocity = shootVelocity;
        _bullet.SetActive(true);
        _bullet.GetComponent<NetworkObject>().Spawn();
        _bullet.GetComponent<BulletController>().attackerID.Value = robotID;
    }
}