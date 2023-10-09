using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;

public class EngineerController : NetworkBehaviour
{
    public bool Enabled = false;

    [Header("Action")]
    private PlayerInput Input;
    private InputAction move;
    private InputAction look;
    private InputAction shoot;
    [SerializeField] private InputAction Boost;
    [SerializeField] private float BoostPower = 240;
    [SerializeField] private InputAction Spin;
    [SerializeField] private bool IsSpin;

    [Header("Chassis")]
    [SerializeField] private GameObject Base;

    private PIDController moveController;
    private PIDController followController;
    [SerializeField] private float[] moveControllerParameters = {0.02f, 0f, 0.015f};
    [SerializeField] private float[] followControllerParameters = {0.02f, 0f, 0.015f};
    
    private float vx;
    private float vy;
    private float vw;

    // Right-Front, Left-Front, Left-Back, Right-Back
    [SerializeField] private WheelController[] Wheels;
    private float[] wheelForce = new float[4];

    [Header("Gimbal")]
    [SerializeField]private GameObject Yaw;
    [SerializeField]private GameObject Pitch;
    private PIDController yawController;
    private PIDController pitchController;
    [SerializeField] private float[] yawControllerParameters = {80f, 0f, 0.015f};
    [SerializeField] private float[] pitchControllerParameters = {50f, 0f, 0.015f};
    [SerializeField] private float yawTargetAngle = 0;
    [SerializeField] private float pitchTargetAngle = 0;

    [SerializeField] private float pitchMax;
    [SerializeField] private float pitchMin;
    
    [Header("Shooter")]
    [SerializeField]private ShooterController Shooter0;
    [SerializeField]private ShooterController Shooter1;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) Enabled = true;
    }

    void Awake()
    {
        if (Input == null)
        {
            Input = new PlayerInput();
        }
    }

    void Start()
    {
        moveController = new PIDController(moveControllerParameters[0],
                                            moveControllerParameters[1],
                                            moveControllerParameters[2]);

        followController = new PIDController(followControllerParameters[0],
                                            followControllerParameters[1],
                                            followControllerParameters[2]);

        yawController = new PIDController(yawControllerParameters[0],
                                        yawControllerParameters[1],
                                        yawControllerParameters[2]);

        pitchController = new PIDController(pitchControllerParameters[0],
                                            pitchControllerParameters[1],
                                            pitchControllerParameters[2]);
    }

    void OnEnable()
    {
        move = Input.Player.Move;
        move.Enable();
        look = Input.Player.Look;
        look.Enable();
        Boost.Enable();
        Spin.Enable();
    }

    void OnDisable()
    {
        move.Disable();
        look.Disable();
        Boost.Disable();
        Spin.Disable();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (!Enabled) return;

        Vector2 lookDelta = look.ReadValue<Vector2>();
        // Debug.Log($"[EngineerController] lookDelta: {lookDelta.x}, {lookDelta.y}");
        
        // Look
        if (Yaw != null)
        {
            if (Yaw.GetComponent<HingeJoint>() != null)
            {
                // yawTargetAngle += lookDelta[0];
                // if (yawTargetAngle < 0) yawTargetAngle += 360;
                // if (yawTargetAngle > 360) yawTargetAngle -= 360;
                // float _yawDifference = yawTargetAngle - Yaw.transform.eulerAngles.y;

                // // Yaw joint exists
                // if (_yawDifference > 180) {
                //     yawMotor.targetVelocity = yawController.Update(_yawDifference - 360, Time.deltaTime);
                // } else if (_yawDifference < -180) {
                //     yawMotor.targetVelocity = yawController.Update(360 + _yawDifference, Time.deltaTime);
                // } else {
                //     yawMotor.targetVelocity = yawController.Update(_yawDifference, Time.deltaTime);
                // }
            } else {
                // No Yaw Joint, update vw only;
                vw = Mathf.Clamp(lookDelta[0] / 100, -1f, 1f);
            }
        }
        
        if (Pitch != null)
        {

        }

        // Move
        Vector2 moveDirection = move.ReadValue<Vector2>();
        // Debug.Log($"[EngineerController] moveDirection: {moveDirection.x}, {moveDirection.y}");

        Vector3 _inputX = moveDirection[0] * Vector3.Project(Yaw.transform.right, Base.transform.right) - moveDirection[1] * Vector3.Project(-Yaw.transform.forward, Base.transform.right);
        Vector3 _inputY = moveDirection[0] * Vector3.Project(Yaw.transform.right, -Base.transform.forward) - moveDirection[1] * Vector3.Project(-Yaw.transform.forward, Base.transform.forward);

        vx = Vector3.Dot(_inputX.normalized, Base.transform.right) * (_inputX).magnitude;
        vy = Vector3.Dot(_inputY.normalized, -Base.transform.forward) * (_inputY).magnitude;

        // 小陀螺 or 底盘跟随云台
        if (IsSpin){
            vw = 1f;
        } else if (Yaw.GetComponent<HingeJoint>() != null) {
            float _angle = Vector3.SignedAngle(Yaw.transform.right, Base.transform.right, Base.transform.up);
            float _vw = followController.Update(_angle, Time.deltaTime);
            vw = -Mathf.Clamp(_vw, -1f, 1f);
            // Debug.Log("vw: " + Mathf.Clamp(_vw, -1f, 1f));
        }

        wheelForce[0] = vx-vy-vw;
        wheelForce[1] = vx+vy+vw;
        wheelForce[2] = vx-vy+vw;
        wheelForce[3] = vx+vy-vw;

        float _factor = 1;
        float powerlimit = gameObject.GetComponent<RefereeController>().PowerLimit.Value;

        if ((Boost.ReadValue<float>() > 0) || powerlimit < 0) _factor = BoostPower / powerlimit;
        _factor = _factor * powerlimit / Wheels.Length;

        for (int i=0; i< Wheels.Length; i++)
        {
            Wheels[i].GetComponent<WheelController>().SetPower(wheelForce[i] * _factor);
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        // 'Use' code here.
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // 'Move' code here.
    }
}
