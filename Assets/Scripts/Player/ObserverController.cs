using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObserverController : MonoBehaviour
{
    [SerializeField] private InputActionReference toggleAction;
    
    public Camera firstPersonCamera;
    public Camera observerCamera;
    
    void OnEnable()
    {
        toggleAction.action.Enable();
        toggleAction.action.performed += ToggleActiveState;
    }

    void OnDisable()
    {
        toggleAction.action.Disable();
        toggleAction.action.performed -= ToggleActiveState;
    }

    private void ToggleActiveState(InputAction.CallbackContext context)
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
