using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObserverController : NetworkBehaviour
{
    public float Speed = 25;

    [Header("Action")]
    private PlayerInput Input;
    private InputAction move;
    private InputAction look;
    private InputAction ascend;
    private InputAction descend;

    void Awake()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        if (Input == null)
        {
            Input = new PlayerInput();
        }
    }

    void OnEnable()
    {
        move = Input.Player.Move;
        look = Input.Player.Look;
        ascend = Input.Player.Ascend;
        descend = Input.Player.Descend;
        
        move.Enable();
        look.Enable();
        ascend.Enable();
        descend.Enable();
    }

    void OnDisable()
    {
        move.Disable();
        look.Disable();
        ascend.Disable();
        descend.Disable();
    }

    void FixedUpdate()
    {
        if (!IsOwner || !IsSpawned) return;

        Vector2 moveInput = move.ReadValue<Vector2>();
        Vector2 lookInput = look.ReadValue<Vector2>();
        float verticalInput = ascend.ReadValue<float>() - descend.ReadValue<float>();  // Add this line

        // Move the character (modified to include vertical movement)
        Vector3 moveDirection = new Vector3(moveInput.x, verticalInput, moveInput.y);  // Modified line
        transform.Translate(moveDirection * Speed * Time.fixedDeltaTime, Space.Self);

        // Rotate the character around the upper axis of the world (Y-axis)
        Vector3 lookDirection = new Vector3(0, lookInput.x, 0);
        transform.Rotate(lookDirection * Speed * Time.fixedDeltaTime, Space.World);

        // Rotate the camera around the right axis of the character (X-axis)
        Vector3 cameraDirection = new Vector3(-lookInput.y, 0, 0);
        transform.Rotate(cameraDirection * Speed * Time.fixedDeltaTime, Space.Self);

        
    }
}
