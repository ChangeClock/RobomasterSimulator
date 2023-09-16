using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class FPVController : MonoBehaviour
{
    [SerializeField] private Camera FPVCamera;
    [SerializeField] private GameObject PlayerUI;

    // This is used to disable vision on Aero side
    [SerializeField] public bool Enabled = true;
    [SerializeField] public int Warning = 0;

    [SerializeField] private TextMeshProUGUI HPBar;
    [SerializeField] public int HPLimit;
    [SerializeField] public int HP;

    // public override void OnStartLocalPlayer()
    // {
    //     if (IsLocalPlayer)
    //     {
    //         FPVCamera.enabled = true;
    //     }
    // }

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        HPBar.text = $"{HP} / {HPLimit}"; 
    }

    public void TurnOnCamera()
    {
        FPVCamera.enabled = true;
        PlayerUI.SetActive(true);
    }
}
