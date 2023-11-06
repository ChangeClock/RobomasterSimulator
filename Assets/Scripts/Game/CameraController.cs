using UnityEngine;

public class CameraController : MonoBehaviour 
{
    private Camera cam;
    private RenderTexture rt;

    void Start() 
    {
        rt = new RenderTexture(640, 480, 32);
        cam = GetComponent<Camera>();
        cam.targetTexture = rt;
    }
}