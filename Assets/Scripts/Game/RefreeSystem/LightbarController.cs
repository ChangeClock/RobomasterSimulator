using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightbarController : MonoBehaviour
{
    public bool Enabled;
    public int Warning;
    public int LightColor;

    // TODO: need to control the percentage of light bar

    private LightController barLight;

    // Start is called before the first frame update
    void Start()
    {
        Transform light = transform.Find("Light");
        if (light != null){
            barLight = light.GetComponent<LightController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (barLight != null){
            barLight.Enabled = Enabled;
            barLight.Warning = Warning;
            barLight.LightColor = LightColor;
        }
    }
}
