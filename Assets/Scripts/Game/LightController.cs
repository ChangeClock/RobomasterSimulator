using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public bool disabled = true;
    public bool warning = false;
    public int lightColor = 0;
    private MeshRenderer meshRenderer;
    private Color purple = new Color(0.57f,0.25f,1f,1f);

    void Start()
    {
        meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (disabled) 
        {
            meshRenderer.material.DisableKeyword("_EMISSION");
        } else {
            meshRenderer.material.EnableKeyword("_EMISSION");
        }

        // TODO: The warning is only applied to lightbar, which needs to have higher priority to life percentage display but lower than the disabled status.
        if (warning)
        {
            meshRenderer.material.SetColor("_EmissionColor", Color.yellow);
            return;
        }

        switch (lightColor)
        {
            case 0:
                meshRenderer.material.SetColor("_EmissionColor", purple);
                return;
            case 1:
                meshRenderer.material.SetColor("_EmissionColor", Color.blue);
                return;
            case 2:
                meshRenderer.material.SetColor("_EmissionColor", Color.red);
                return;
            default:
                Debug.LogWarning("[ArmorController] Unknown armor light type");
                return;
        }
    }
}
