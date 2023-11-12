using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;

public class RobotController : NetworkBehaviour
{
    private RefereeController referee;

    public NetworkVariable<bool> Enabled = new NetworkVariable<bool>(false);

    public NetworkVariable<bool> AutoOperate = new NetworkVariable<bool>(false);

    [Header("Action")]
    private PlayerInput Input;
    private InputAction move;
    private InputAction look;
    private InputAction shoot;
    [SerializeField] private InputAction Aim;
    [SerializeField] private InputAction Boost;
    [SerializeField] private float BoostPower = 240;
    [SerializeField] private InputAction Spin;

    public NetworkVariable<bool> IsBoost = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> IsShoot = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> IsSpin = new NetworkVariable<bool>(false);

    [Header("Chassis")]
    [SerializeField] private GameObject Base;
    private Vector3 LastPostion;
    private Quaternion LastRotation;

    public NetworkVariable<float> PowerRedundancies = new NetworkVariable<float>(5.0f);

    private PIDController moveControllerX;
    private PIDController moveControllerY;
    private PIDController followController;
    [SerializeField] private float[] moveControllerParameters = {0.02f, 0f, 0.015f};
    [SerializeField] private float[] followControllerParameters = {0.02f, 0f, 0.015f};
    public NetworkVariable<float> moveDirectionX = new NetworkVariable<float>(0);
    public NetworkVariable<float> moveDirectionY = new NetworkVariable<float>(0);
    
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
    public NetworkVariable<float> currentYawAngle = new NetworkVariable<float>(0);
    public NetworkVariable<float> yawTargetAngle = new NetworkVariable<float>(0);
    public NetworkVariable<float> yawPatrolSpeed = new NetworkVariable<float>(60f);
    public NetworkVariable<float> pitchTargetAngle = new NetworkVariable<float>(0);
    public NetworkVariable<float> currentPitchAngle = new NetworkVariable<float>(0);

    [SerializeField] private float pitchMax;
    [SerializeField] private float pitchMin;
    
    [Header("Shooter")]
    [SerializeField]private List<ShooterController> ShooterList = new List<ShooterController>();
    [SerializeField]private int CurrentShooter = 0;
    [SerializeField]private float ShootFrequency = 25f;
    private float _shootTimeoutDelta = 0f;

    [Header("Camera")]
    [SerializeField] private List<CameraController> CameraList = new List<CameraController>();

    private Dictionary<int, TargetInfo> TargetList = new Dictionary<int, TargetInfo>();

    public class TargetInfo
    {
        public int ID;
        public Faction Faction;
        public Vector3 Position;
        public float LastTime;
        public float LiveTime = 1.0f;

        public TargetInfo(int id, Faction faction, Vector3 position)
        {
            ID = id;
            Faction = faction;
            Position = position;
            LastTime = LiveTime;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        { 
            Enabled.Value = true;
            if (!AutoOperate.Value) UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Awake()
    {
        if (Input == null)
        {
            Input = new PlayerInput();
        }

        if (Yaw!=null) yawTargetAngle.Value = Yaw.transform.eulerAngles.y;
        if (Pitch!=null) pitchTargetAngle.Value = Pitch.transform.eulerAngles.z;

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

        Boost.performed += context =>
        {
            IsBoost.Value = !IsBoost.Value;
        };

        Boost.canceled += context =>
        {
            IsBoost.Value = !IsBoost.Value;
        };

        shoot.performed += context =>
        {
            IsShoot.Value = !IsShoot.Value;
        };

        shoot.canceled += context =>
        {
            IsShoot.Value = !IsShoot.Value;
        };

        Spin.performed += context =>
        {
            IsSpin.Value = !IsSpin.Value;
        };
    }

    void OnEnable()
    {
        move = Input.Player.Move;
        look = Input.Player.Look;
        shoot = Input.Player.Shoot;

        InputEnable();

        foreach (var cam in CameraList)
        {
            cam.OnTargetDetected += TargetDetectedHandler;
        }
    }

    void OnDisable()
    {
        InputDisable();

        foreach (var cam in CameraList)
        {
            cam.OnTargetDetected += TargetDetectedHandler;
        }
    }

    void FixedUpdate()
    {
        if (!Enabled.Value) return;

        if (IsOwner)
        {
            if (!AutoOperate.Value)
            {
                Vector2 lookDelta = look.ReadValue<Vector2>();
                // Debug.Log($"[EngineerController] lookDelta: {lookDelta.x}, {lookDelta.y}");
                
                Vector2 moveDirection = move.ReadValue<Vector2>();
                moveDirectionX.Value = moveDirection.x;
                moveDirectionY.Value = moveDirection.y;
                // Debug.Log($"[EngineerController] moveDirection: {moveDirection.x}, {moveDirection.y}");

                // Debug.Log($"[RobotController] CursorLock: {UnityEngine.Cursor.lockState}");

                // Rotate to the last target position
                if (UnityEngine.Cursor.lockState != CursorLockMode.Locked)
                {
                    lookDelta = Vector2.zero;
                }

                if (Aim.ReadValue<float>() <= 0 || FindClosestTarget() == Vector3.zero)
                {
                    yawTargetAngle.Value += lookDelta.x;
                    pitchTargetAngle.Value += lookDelta.y;
                } else {
                    LockTarget(FindClosestTarget());
                }
            } else {
                IsSpin.Value = true;

                if (FindClosestTarget() != Vector3.zero)
                {
                    LockTarget(FindClosestTarget());
                    IsShoot.Value = true;
                } else {
                    IsShoot.Value = false;
                    // loop yaw from 0 - 360, pitch from pitchmin - pitchmax
                    yawTargetAngle.Value += yawPatrolSpeed.Value * Time.deltaTime;
                    pitchTargetAngle.Value = 0;
                }
            }
            
            // Debug.Log($"[RobotController] yawTargetAngle {yawTargetAngle.Value} pitchTargetAngle {pitchTargetAngle.Value}");

            TickTarget();
        }

        if (IsServer)
        {
            if (Yaw != null)
            {
                if (Yaw.GetComponent<HingeJoint>() != null)
                {
                    yawController.updatePara(yawControllerParameters[0],
                                            yawControllerParameters[1],
                                            yawControllerParameters[2]);
                                            
                    JointMotor yawMotor = Yaw.GetComponent<HingeJoint>().motor;

                    if (yawTargetAngle.Value < 0) yawTargetAngle.Value += 360;
                    if (yawTargetAngle.Value > 360) yawTargetAngle.Value -= 360;

                    currentYawAngle.Value = Yaw.transform.eulerAngles.y;
                    float _yawDifference = yawTargetAngle.Value - currentYawAngle.Value;

                    // Yaw joint exists
                    if (_yawDifference > 180) {
                        _yawDifference -= 360;
                    } else if (_yawDifference < -180) {
                        _yawDifference += 360;
                    }

                    yawMotor.targetVelocity = yawController.Update(_yawDifference, Time.deltaTime);

                    Yaw.transform.Rotate(new Vector3(0, yawMotor.targetVelocity * Time.deltaTime, 0), Space.World);

                    // Debug.Log($"[RobotController] yawTargetAngle {yawTargetAngle} yawDifference {_yawDifference} Yaw {Yaw.transform.eulerAngles.y} targetVelocity {yawMotor.targetVelocity}");

                    // Yaw.GetComponent<HingeJoint>().motor = yawMotor;
                } else {
                    // No Yaw Joint, update vw only;
                    targetVw = Mathf.Clamp(yawTargetAngle.Value / 100, -1f, 1f);
                }

                // Debug.DrawLine(Yaw.transform.position, Yaw.transform.position + Yaw.transform.right * 10, Color.blue);
            }
            
            if (Pitch != null)
            {
                if (Pitch.GetComponent<HingeJoint>() != null)
                {
                    pitchController.updatePara(pitchControllerParameters[0],
                                            pitchControllerParameters[1],
                                            pitchControllerParameters[2]);

                    JointMotor pitchMotor = Pitch.GetComponent<HingeJoint>().motor;

                    if (pitchTargetAngle.Value < pitchMin) pitchTargetAngle.Value = pitchMin;
                    if (pitchTargetAngle.Value > pitchMax) pitchTargetAngle.Value = pitchMax;

                    float _currentPitchAngle = Pitch.transform.eulerAngles.z;
                    
                    if (_currentPitchAngle > 180) {
                        _currentPitchAngle -= 360;
                    } else if (_currentPitchAngle < -180) {
                        _currentPitchAngle += 360;
                    }

                    currentPitchAngle.Value = _currentPitchAngle;

                    float _pitchDifference = pitchTargetAngle.Value - currentPitchAngle.Value;

                    // Debug.Log($"[RobotController] pitchTargetAngle {pitchTargetAngle} pitchDifference {_pitchDifference} Pitch {Pitch.transform.eulerAngles.z}");

                    pitchMotor.targetVelocity = pitchController.Update(_pitchDifference, Time.deltaTime);

                    Pitch.GetComponent<HingeJoint>().motor = pitchMotor;
                }

                Debug.DrawLine(Pitch.transform.position, Pitch.transform.position + Pitch.transform.right * 10, Color.green);
            }

            // Move
            // Debug.DrawLine(Yaw.transform.position, Yaw.transform.position - Yaw.transform.forward * moveDirectionX.Value * 100, Color.yellow);
            // Debug.DrawLine(Yaw.transform.position, Yaw.transform.position + Yaw.transform.right * moveDirectionY.Value * 100, Color.blue);

            Vector3 x_Bx = moveDirectionX.Value * Vector3.Project(- Yaw.transform.forward, Base.transform.right);
            Vector3 x_By = moveDirectionX.Value * Vector3.Project(- Yaw.transform.forward, Base.transform.forward);
            // Debug.DrawLine(Base.transform.position, Base.transform.position + x_Bx * 100, Color.yellow);
            // Debug.DrawLine(Base.transform.position, Base.transform.position + x_By * 100, Color.blue);

            Vector3 y_Bx = moveDirectionY.Value * Vector3.Project(Yaw.transform.right, Base.transform.right);
            Vector3 y_By = moveDirectionY.Value * Vector3.Project(Yaw.transform.right, Base.transform.forward);
            // Debug.DrawLine(Base.transform.position, Base.transform.position + y_Bx * 100, Color.yellow);
            // Debug.DrawLine(Base.transform.position, Base.transform.position + y_By * 100, Color.blue);

            Vector3 _inputX = x_Bx + y_Bx;
            Vector3 _inputY = x_By + y_By;
            // Debug.DrawLine(Base.transform.position, Base.transform.position + _inputX * 100, Color.yellow);
            // Debug.DrawLine(Base.transform.position, Base.transform.position + _inputY * 100, Color.blue);

            targetVx = Vector3.Dot(_inputX.normalized, Base.transform.right) * (_inputX).magnitude;
            targetVy = - Vector3.Dot(_inputY.normalized, Base.transform.forward) * (_inputY).magnitude;
            // Debug.Log($"[RobotController] targetVx {targetVx} targetVy {targetVy}");

            moveControllerX = new PIDController(moveControllerParameters[0],
                                                moveControllerParameters[1],
                                                moveControllerParameters[2]);

            moveControllerY = new PIDController(moveControllerParameters[0],
                                                moveControllerParameters[1],
                                                moveControllerParameters[2]);

            // 小陀螺 or 底盘跟随云台
            if (IsSpin.Value){
                targetVw = 0.5f;
            } else if (Yaw.GetComponent<HingeJoint>() != null) {
                followController.updatePara(followControllerParameters[0],
                                            followControllerParameters[1],
                                            followControllerParameters[2]);
                
                float _angle = Vector3.SignedAngle(Yaw.transform.right, Base.transform.right, Base.transform.up);

                // float _vw = - (yawTargetAngle - lastYawTargetAngle) + followController.Update(_angle, Time.deltaTime);
                // float _vw = - (1/Time.deltaTime) * (yawTargetAngle - lastYawTargetAngle) * feedForwardFactor + followController.Update(_angle, Time.deltaTime);
                float _vw = followController.Update(_angle, Time.deltaTime);
                
                targetVw = -Mathf.Clamp(_vw, -0.5f, 0.5f);
                // Debug.Log("vw: " + targetVw);
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
            // vx = targetVx;
            // vy = targetVy;
            vw = targetVw;

            // Vector3 _velocity = new Vector3(vx, vy, vw).normalized;
            // vx = _velocity.x;
            // vy = _velocity.y;
            // vw = _velocity.z;

            // Debug.Log($"[RobotController] X {targetVx} / {deltaX/Time.deltaTime} Y {targetVy} / {deltaY/Time.deltaTime}");
            // Debug.Log($"[RobotController] vx {vx} vy {vy} vw {vw}");

            wheelForce[0] = vx-vy-vw;
            wheelForce[1] = vx+vy+vw;
            wheelForce[2] = vx-vy+vw;
            wheelForce[3] = vx+vy-vw;

            float _factor = 1;
            float powerlimit = referee.PowerLimit.Value - PowerRedundancies.Value;

            if (IsBoost.Value || powerlimit < 0) _factor = BoostPower / powerlimit;
            _factor = _factor * powerlimit / Wheels.Length;

            for (int i=0; i< Wheels.Length; i++)
            {
                Wheels[i].GetComponent<WheelController>().SetPower(wheelForce[i] * _factor);
            }

            if (ShooterList.Count > 0)
            {
                if (IsShoot.Value)
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

    void InputEnable()
    {
        move.Enable();
        look.Enable();
        shoot.Enable();
        Aim.Enable();
        Boost.Enable();
        Spin.Enable();
    }

    void InputDisable()
    {
        move.Disable();
        look.Disable();
        shoot.Disable();
        Aim.Disable();
        Boost.Disable();
        Spin.Disable();
    }

    bool HasHeatRedundancy(ShooterController shooter)
    {
        return shooter.HeatLimit.Value - shooter.Heat.Value >= (shooter.Mode.Value == 0 ? 10 : 100);
    }

    void TargetDetectedHandler(int id, Faction fac, Vector3 position)
    {
        // Debug.Log($"[RobotController] TargetDetectedHandler {id} {fac} {position}");
    
        if (fac == Faction.Neu) return;
        if (fac == referee.faction.Value) return;

        if (id == 0) return;

        if (TargetList.ContainsKey(id))
        {
            TargetList[id].LastTime = TargetList[id].LiveTime;
            TargetList[id].Position = position;
        } else {
            TargetList.Add(id, new TargetInfo(id, fac, position));
        }
    }

    Vector3 FindClosestTarget()
    {
        Vector3 _position = Vector3.zero;
        float _distance = 1000f;

        if (TargetList.Count <= 0) return _position;

        foreach (var target in TargetList.Values)
        {
            if (target.LastTime <= 0) continue;

            float _d = Vector3.Distance(Pitch.transform.position, target.Position);
            if (_d < _distance)
            {
                _distance = _d;
                _position = target.Position;
            }

            Debug.Log($"[RobotController] FindTarget {_distance}");
        }

        Debug.Log($"[RobotController] FindClosestTarget {_distance}");

        return _position;
    }

    void LockTarget(Vector3 target)
    {
        // Projectile the target position to the yaw and pitch plane
        Vector3 _target = (target - Pitch.transform.position).normalized;
        Vector3 _yawTarget = Vector3.ProjectOnPlane(_target, Yaw.transform.up);
        Vector3 _pitchTarget = Vector3.ProjectOnPlane(_target, Pitch.transform.forward);
        
        float _targetYawDifference = Vector3.SignedAngle(Yaw.transform.right, _yawTarget, Yaw.transform.up);
        float _targetPitchDifference = Vector3.SignedAngle(Pitch.transform.right, _pitchTarget, Pitch.transform.forward);

        yawTargetAngle.Value = currentYawAngle.Value + _targetYawDifference;
        pitchTargetAngle.Value = currentPitchAngle.Value + _targetPitchDifference;

        // Debug.DrawLine(Yaw.transform.position, Yaw.transform.position + _yawTarget * 10, Color.red);
        // Debug.DrawLine(Pitch.transform.position, Pitch.transform.position + _pitchTarget * 10, Color.red);

        // Debug.Log($"[RobotController] _yawDifference {_targetYawDifference} _pitchDifference {_targetPitchDifference} ");
        // Debug.Log($"[RobotController] currentYawAngle {currentYawAngle.Value} currentPitchAngle {currentPitchAngle.Value} ");
    }

    void TickTarget()
    {
        if (TargetList.Count <= 0) return;

        List<int> overtimeTargets = new List<int>();

        foreach (var target in TargetList.Values)
        {
            target.LastTime -= Time.deltaTime;
            if (target.LastTime <= 0)
            {
                overtimeTargets.Add(target.ID);
            }
            Debug.DrawLine(Pitch.transform.position, target.Position, Color.cyan);
        }

        if (overtimeTargets.Count <= 0) return;

        foreach (var id in overtimeTargets)
        {
            TargetList.Remove(id);
        }
    }
}
