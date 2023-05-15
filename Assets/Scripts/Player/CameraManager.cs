using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera firstPersonCamera;
    public Camera observerCamera;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (firstPersonCamera.enabled)
            {
                firstPersonCamera.enabled = false;
                observerCamera.enabled = true;
            }
            else
            {
                firstPersonCamera.enabled = true;
                observerCamera.enabled = false;
            }
        }
    }
}
