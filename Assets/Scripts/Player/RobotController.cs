using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;

public class RobotController : NetworkBehaviour
{
    public bool Enabled = false;

    private RefereeController referee;

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
    private Vector3 LastPostion;
    private Quaternion LastRotation;

    private PIDController moveControllerX;
    private PIDController moveControllerY;
    private PIDController followController;
    [SerializeField] private float[] moveControllerParameters = {0.02f, 0f, 0.015f};
    [SerializeField] private float[] followControllerParameters = {0.02f, 0f, 0.015f};
    
    private float targetVx;
    private float targetVy;
    private float targetVw;

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

    [SerializeField] private float yawDeathZone = 0.1f;

    [SerializeField] private float pitchMax;
    [SerializeField] private float pitchMin;
    
    [Header("Shooter")]
    [SerializeField]private List<ShooterController> ShooterList = new List<ShooterController>();
    [SerializeField]private int CurrentShooter = 0;
    [SerializeField]private float ShootFrequency = 25f;
    private float _shootTimeoutDelta = 0f;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        { 
            Enabled = true;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Awake()
    {
        if (Input == null)
        {
            Input = new PlayerInput();
        }

        referee = gameObject.GetComponent<RefereeController>();
    }

    void Start()
    {
        moveControllerX = new PIDController(moveControllerParameters[0],
                                            moveControllerParameters[1],
                                            moveControllerParameters[2]);

        moveControllerY = new PIDController(moveControllerParameters[0],
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
    
        LastPostion = Base.transform.position;

        Spin.performed += context =>
        {
            IsSpin = !IsSpin;
        };
    }

    void OnEnable()
    {
        move = Input.Player.Move;
        move.Enable();
        look = Input.Player.Look;
        look.Enable();
        shoot = Input.Player.Shoot;
        shoot.Enable();
        Boost.Enable();
        Spin.Enable();
    }

    void OnDisable()
    {
        move.Disable();
        look.Disable();
        shoot.Disable();
        Boost.Disable();
        Spin.Disable();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (!Enabled) return;

        Vector2 lookDelta = look.ReadValue<Vector2>();
        // Debug.Log($"[EngineerController] lookDelta: {lookDelta.x}, {lookDelta.y}");
        
        Vector2 moveDirection = move.ReadValue<Vector2>();
        // Debug.Log($"[EngineerController] moveDirection: {moveDirection.x}, {moveDirection.y}");

        // Debug.Log($"[RobotController] CursorLock: {UnityEngine.Cursor.lockState}");

        if (UnityEngine.Cursor.lockState != CursorLockMode.Locked)
        {
            lookDelta = Vector2.zero;
            moveDirection = Vector2.zero;
        }

        ControlServerRpc(lookDelta, moveDirection, IsSpin, Boost.ReadValue<float>(), shoot.ReadValue<float>());
    }

    bool HasHeatRedundancy(ShooterController shooter)
    {
        return shooter.HeatLimit.Value - shooter.Heat.Value >= (shooter.Mode.Value == 0 ? 10 : 100);
    }

    [ServerRpc]
    void ControlServerRpc(Vector2 lookDelta, Vector2 moveDirection, bool isSpin, float boostTrigger, float shootTrigger, ServerRpcParams serverRpcParams = default)
    {
        // Look
        if (Yaw != null)
        {
            if (Yaw.GetComponent<HingeJoint>() != null)
            {
                yawController.updatePara(yawControllerParameters[0],
                                        yawControllerParameters[1],
                                        yawControllerParameters[2]);

                JointMotor yawMotor = Yaw.GetComponent<HingeJoint>().motor;

                yawTargetAngle += lookDelta[0];
                if (yawTargetAngle < 0) yawTargetAngle += 360;
                if (yawTargetAngle > 360) yawTargetAngle -= 360;
                float _yawDifference = yawTargetAngle - Yaw.transform.eulerAngles.y;

                // Yaw joint exists
                if (_yawDifference > 180) {
                    yawMotor.targetVelocity = yawController.Update(_yawDifference - 360, Time.deltaTime);
                } else if (_yawDifference < -180) {
                    yawMotor.targetVelocity = yawController.Update(360 + _yawDifference, Time.deltaTime);
                } else {
                    yawMotor.targetVelocity = yawController.Update(_yawDifference, Time.deltaTime);
                }

                Yaw.GetComponent<HingeJoint>().motor = yawMotor;
            } else {
                // No Yaw Joint, update vw only;
                targetVw = Mathf.Clamp(lookDelta[0] / 100, -1f, 1f);
            }
        }
        
        if (Pitch != null)
        {
            if (Pitch.GetComponent<HingeJoint>() != null)
            {
                pitchController.updatePara(pitchControllerParameters[0],
                                        pitchControllerParameters[1],
                                        pitchControllerParameters[2]);

                JointMotor pitchMotor = Pitch.GetComponent<HingeJoint>().motor;

                pitchTargetAngle += lookDelta[1];
                if (pitchTargetAngle < pitchMin) pitchTargetAngle = pitchMin;
                if (pitchTargetAngle > pitchMax) pitchTargetAngle = pitchMax;
                float _pitchDifference = pitchTargetAngle - Pitch.transform.eulerAngles.z;

                pitchMotor.targetVelocity = pitchController.Update(_pitchDifference, Time.deltaTime);

                Pitch.GetComponent<HingeJoint>().motor = pitchMotor;
            }
        }

        // Move
        Vector3 _inputX = moveDirection[0] * Vector3.Project(Yaw.transform.forward, Base.transform.right) + moveDirection[1] * Vector3.Project(Yaw.transform.right, Base.transform.right);
        Vector3 _inputY = moveDirection[0] * Vector3.Project(Yaw.transform.forward, Base.transform.forward) + moveDirection[1] * Vector3.Project(Yaw.transform.right, Base.transform.forward);

        targetVx = Vector3.Dot(_inputX.normalized, Base.transform.right) * (_inputX).magnitude;
        targetVy = Vector3.Dot(_inputY.normalized, Base.transform.forward) * (_inputY).magnitude;

        moveControllerX = new PIDController(moveControllerParameters[0],
                                            moveControllerParameters[1],
                                            moveControllerParameters[2]);

        moveControllerY = new PIDController(moveControllerParameters[0],
                                            moveControllerParameters[1],
                                            moveControllerParameters[2]);

        // 小陀螺 or 底盘跟随云台
        if (isSpin){
            targetVw = 1f;
        } else if (Yaw.GetComponent<HingeJoint>() != null) {
            followController.updatePara(followControllerParameters[0],
                                        followControllerParameters[1],
                                        followControllerParameters[2]);
            
            float _angle = Vector3.SignedAngle(Yaw.transform.right, Base.transform.right, Base.transform.up);

            if (Mathf.Abs(_angle) < yawDeathZone) _angle = 0;

            float _vw = followController.Update(_angle, Time.deltaTime);
            targetVw = -Mathf.Clamp(_vw, -1f, 1f);
            // Debug.Log("vw: " + Mathf.Clamp(_vw, -1f, 1f));
        }

        Vector3 deltaPostion = Base.transform.position - LastPostion;
        float deltaW = (Base.transform.rotation.eulerAngles.y - LastRotation.eulerAngles.y) / Time.deltaTime;
        LastPostion = Base.transform.position;
        float deltaX = Vector3.Dot(deltaPostion.normalized, Base.transform.right) * (deltaPostion).magnitude;
        float deltaY = Vector3.Dot(deltaPostion.normalized, Base.transform.forward) * (deltaPostion).magnitude;;

        // Debug.Log($"[RobotContorller] deltaPostion {deltaPostion}");

        // Debug.Log($"[RobotController] targetVx {targetVx} targetVy {targetVy} targetVw {targetVw}");
        // Debug.Log($"[RobotController] deltaX {deltaX} deltaY {deltaY} deltaW {deltaW}");

        vx = Mathf.Clamp(moveControllerX.Update(targetVx - deltaX, Time.deltaTime), -1f, 1f);
        vy = Mathf.Clamp(moveControllerY.Update(targetVy + deltaY, Time.deltaTime), -1f, 1f);
        vw = targetVw;

        // Debug.Log($"[RobotController] X {targetVx} / {deltaX/Time.deltaTime} Y {targetVy} / {deltaY/Time.deltaTime}");
        // Debug.Log($"[RobotController] vx {vx} vy {vy}");

        wheelForce[0] = vx-vy-vw;
        wheelForce[1] = vx+vy+vw;
        wheelForce[2] = vx-vy+vw;
        wheelForce[3] = vx+vy-vw;

        float _factor = 1;
        float powerlimit = referee.PowerLimit.Value;

        if ((boostTrigger > 0) || powerlimit < 0) _factor = BoostPower / powerlimit;
        _factor = _factor * powerlimit / Wheels.Length;

        for (int i=0; i< Wheels.Length; i++)
        {
            Wheels[i].GetComponent<WheelController>().SetPower(wheelForce[i] * _factor);
        }

        if (ShooterList.Count > 0)
        {
            if (shootTrigger > 0)
            {
                if (_shootTimeoutDelta > 1 / ShootFrequency)
                {
                    if (HasHeatRedundancy(ShooterList[CurrentShooter]))
                    {
                        ShooterList[CurrentShooter].PullTrigger();
                        _shootTimeoutDelta = 0f;
                    } else {
                        int i = 0;
                        foreach (var shooter in ShooterList)
                        {
                            if (HasHeatRedundancy(shooter)) CurrentShooter = i;
                            i ++;
                        }
                    }
                } else {
                    _shootTimeoutDelta += Time.deltaTime;
                }
            }
        }
    }
}
