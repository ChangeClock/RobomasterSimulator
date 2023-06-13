using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightbarController : MonoBehaviour
{
    public bool disabled;
    public int lightColor;

    private LightController armorLight;

    // Start is called before the first frame update
    void Start()
    {
        Transform light = transform.Find("Light");
        if (light != null){
            armorLight = light.GetComponent<LightController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (armorLight != null){
            armorLight.disabled = disabled;
            armorLight.lightColor = lightColor;
        }
    }
}
