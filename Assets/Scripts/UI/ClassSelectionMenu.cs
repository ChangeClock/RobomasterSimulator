using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClassSelectionMenu : MonoBehaviour 
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private Camera SurroundCamera;

    // ClassSelection Menu
    [SerializeField] private Button[] RedClasses;
    [SerializeField] private Button[] BlueClasses;
    [SerializeField] private Button[] RefereeClasses;
    [SerializeField] private Button[] Oberver;

    // [SerializeField] public NetworkPrefabsList[] prefabList;
    [SerializeField] public GameObject[] prefabList;

    void Start()
    {
        for (int i = 0; i < RedClasses.Length; i ++)
        {
            int tempVar = i;
            RedClasses[i].onClick.AddListener(() => SpawnPlayer(1,tempVar));
        }

        for (int i = 0; i < BlueClasses.Length; i ++)
        {
            int tempVar = i;
            BlueClasses[i].onClick.AddListener(() => SpawnPlayer(2,tempVar));
        }

        for (int i = 0; i < Oberver.Length; i ++)
        {
            int tempVar = i;
            BlueClasses[i].onClick.AddListener(() => SpawnPlayer(0,tempVar));
        }

        // foreach(var (item, index) in Oberver.WithIndex())
        // {
        //     item.onClick.AddListener(() => SpawnPlayer(0,index));
        // }

        // foreach(var (item, index) in RedClasses.WithIndex())
        // {
        //     item.onClick.AddListener(() => SpawnPlayer(1,index));
        // }

        // foreach(var (item, index) in BlueClasses.WithIndex())
        // {
        //     item.onClick.AddListener(() => SpawnPlayer(2,index));
        // }

        // foreach(var (item, index) in RefereeClasses.WithIndex())
        // {
        //     item.onClick.AddListener(() => SpawnPlayer(3,index));
        // }
    }

    void Update()
    {

    }

    void SpawnPlayer(int role = 0, int id = 0)
    {
        // role: 0-observer 1-red 2-blue 3-referee

        // switch(role)
        // {
        //     case 0:
        //     case 3:

        //         break;
        //     case 1:
        //     case 2:
        //         SpawnServerRpc(role, id);
        //         break;
        // }

        spawnManager.SpawnServerRpc(role, id);

        gameObject.SetActive(false);
        SurroundCamera.enabled = false;
    }
}