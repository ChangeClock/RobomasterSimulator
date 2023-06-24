using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraController : MonoBehaviour
{
    [SerializeField] public bool Enabled = true;
    [SerializeField] public int Warning = 0;

    // Header
    [SerializeField] private TextMeshProUGUI TimeBar;
    [SerializeField] public int TimePast;

    [SerializeField] private TextMeshProUGUI ROutpostHPBar;
    [SerializeField] public int ROutpostHPLimit;
    [SerializeField] public int ROutpostHP;

    [SerializeField] private TextMeshProUGUI BOutpostHPBar;
    [SerializeField] public int BOutpostHPLimit;
    [SerializeField] public int BOutpostHP;

    [SerializeField] private TextMeshProUGUI RBaseHPBar;
    [SerializeField] public int RBaseShieldLimit;
    [SerializeField] public int RBaseShield;
    [SerializeField] public int RBaseHPLimit;
    [SerializeField] public int RBaseHP;

    [SerializeField] private TextMeshProUGUI BBaseHPBar;
    [SerializeField] public int BBaseShieldLimit;
    [SerializeField] public int BBaseShield;
    [SerializeField] public int BBaseHPLimit;
    [SerializeField] public int BBaseHP;

    [SerializeField] private TextMeshProUGUI HPBar;
    [SerializeField] public int HPLimit;
    [SerializeField] public int HP;

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        HPBar.text = $"{HP} / {HPLimit}"; 
        ROutpostHPBar.text = $"{ROutpostHP} / {ROutpostHPLimit}";
        BOutpostHPBar.text = $"{BOutpostHP} / {BOutpostHPLimit}";
    }
}
