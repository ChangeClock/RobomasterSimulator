using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.Networking;
using Cinemachine;

public class RobotController : NetworkBehaviour
{
    public float rotateSpeed;
    public float motorTorque;

    // private PlayerInputs _input;
    private float[] _input = new float[6];

    [Header("Chassis")]
    private Transform Base;

    private PIDController followController;
    [SerializeField] private float[] followControllerParameters = {0.02f, 0f, 0.015f};
    
    private bool[] wheelsIsGrounded = new bool[4];
    private Transform[] wheels = new Transform[4];
    private float[] wheelForce = new float[4];
    private Vector3[] wheelForceDirection = new Vector3[4];

    private bool isSpin = false;

    [Header("Gimbal")]
    [SerializeField] private float yawTargetAngle = 0;
    [SerializeField] private float pitchTargetAngle = 0;
    private PIDController yawController;
    private PIDController pitchController;
    private Transform yawComponent;
    private Transform pitchComponent;
    private HingeJoint yawJoint;
    private HingeJoint pitchJoint;
    private JointMotor yawMotor;
    private JointMotor pitchMotor;
    
    [Tooltip("Shoot Frequency in HZ")]
    public float ShootFrequency = 20f;
    public float ShootSpeed = 30f;
    public float ShootHeight = 1.5f;
    public delegate void ShootAction(Vector3 userPosition, Quaternion userDirection, Vector3 ShootVelocity, int ShooterType);
    public static event ShootAction OnShoot;

    [Header("Player Shooter")]
    [Tooltip("If true, the player can shoot")]
    public bool ShooterEnabled = true;
    [Tooltip("0: 17mm; 1: 42mm")]
    public int ShooterType = 0;
    private float _shootTimeoutDelta;

    public override void OnNetworkSpawn()
    {
        // Debug.Log("Client:" + NetworkManager.Singleton.LocalClientId + "IsOwner?" + IsOwner);
        if (IsOwner) {
            this.gameObject.GetComponent<PlayerInput>().enabled = true;
            this.gameObject.GetComponentInChildren<CinemachineVirtualCamera>().enabled = true;
        }
    }

    void Start()
    {

        Base = transform.Find("Base");

        yawComponent = transform.Find("Yaw");
        pitchComponent = transform.Find("Pitch");

        followController = new PIDController(followControllerParameters[0],
                                            followControllerParameters[1],
                                            followControllerParameters[2]);

        wheels[0] = transform.Find("Right-Front-Wheel");
        wheels[1] = transform.Find("Left-Front-Wheel");
        wheels[2] = transform.Find("Left-Back-Wheel");
        wheels[3] = transform.Find("Right-Back-Wheel");

        if (yawComponent != null)
        {
            yawJoint = yawComponent.GetComponent<HingeJoint>();
            yawMotor = yawJoint.motor;
        }
        if (pitchComponent != null)
        {
            pitchJoint = pitchComponent.GetComponent<HingeJoint>();
            pitchMotor = pitchJoint.motor;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        StarterAssets.StarterAssetsInputs _playerInputs = GetComponent<StarterAssets.StarterAssetsInputs>();

        _input[0] = _playerInputs.move.x;
        _input[1] = _playerInputs.move.y;
        _input[2] = _playerInputs.look.x;
        _input[3] = _playerInputs.look.y;
        _input[4] = _playerInputs.shoot ? 1f : 0f;
        _input[5] = _playerInputs.spin ? 1f : 0f;

        _playerInputs.shoot = false;
        _playerInputs.spin = false;

        // All input status
        // Debug.Log(_input[0] + " " + _input[1] + " " + _input[2] + " " + _input[3] + " " + _input[4] + " " + _input[5]);

        ControlRobotServerRpc(_input);
    }

    private void MoveSight()
    {
        // TODO: YAW和PITCH都得上PID

        followController.updatePara(followControllerParameters[0],
                                    followControllerParameters[1],
                                    followControllerParameters[2]);

        yawTargetAngle += _input[2];
        if (yawTargetAngle < 0) yawTargetAngle += 360;
        if (yawTargetAngle > 360) yawTargetAngle -= 360;

        pitchTargetAngle += _input[3];
        float _yawDifference = yawTargetAngle - yawComponent.eulerAngles.y;

        // Debug.Log("difference: " + _yawDifference);
        // Debug.Log("pitch: " + pitchTargetAngle);

        pitchMotor.targetVelocity = -_input[3] * rotateSpeed;
        pitchJoint.motor = pitchMotor;

        if (_yawDifference > 180) {
            yawMotor.targetVelocity = 10 * (_yawDifference - 360);
        } else if (_yawDifference < -180) {
            yawMotor.targetVelocity = 10 * (360 + _yawDifference);
        } else {
            yawMotor.targetVelocity = 10 * _yawDifference;
        }
        // Debug.Log("Velocity: " + yawMotor.targetVelocity);
        
        yawJoint.motor = yawMotor;
    }

    private void Move()
    {
        float vx = -_input[1];
        float vy = _input[0];
        float vw = 0;

        // 小陀螺 or 底盘跟随云台
        if (isSpin){
            vw = 1f;
        } else {
            float _angle = Vector3.SignedAngle(yawComponent.right, Base.right, Base.up);
            float _vw = followController.Update(_angle, Time.deltaTime);
            vw = -Mathf.Clamp(_vw, -1f, 1f);
            // Debug.Log("vw: " + Mathf.Clamp(_vw, -1f, 1f));
        }

        wheelForce[0] = -vx-vy-vw;
        wheelForce[1] = vx-vy-vw;
        wheelForce[2] = vx+vy-vw;
        wheelForce[3] = -vx+vy-vw;

        wheelForceDirection[0] = Base.right + Base.forward; // (-1,0,1)
        wheelForceDirection[1] = -Base.right + Base.forward; // (-1,0,-1)
        wheelForceDirection[2] = -Base.right - Base.forward; // (1,0,-1)
        wheelForceDirection[3] = Base.right - Base.forward; // (1,0,1)

        for (int i=0; i<4; i++)
        {
            // 捕捉每个轮子的碰撞状态，设置是否触底，根据触底与否再施加力
            // Debug.Log("wheel " + i + " is colliding? : " + wheels[i].GetComponent<WheelController>().IsColliding());
            wheels[i].GetComponent<Rigidbody>().AddForce(wheelForce[i] * wheelForceDirection[i] * motorTorque * (wheels[i].GetComponent<WheelController>().IsColliding() ? 1 : 0));
            Debug.DrawLine(wheels[i].position, wheels[i].position + (wheelForceDirection[i] * wheelForce[i] * (wheels[i].GetComponent<WheelController>().IsColliding() ? 1 : 0) * 25f) , Color.red);
        }
    }

    private void Shoot()
    {
        if (_input[4] > 0 && _shootTimeoutDelta <= 0.0f && ShooterEnabled) 
        {
            // reset the shoot timeout timer
            _shootTimeoutDelta = 1 / ShootFrequency;

            // Debug.Log("Shoot");
            if (OnShoot != null){
                Vector3 _shootOffset = Vector3.zero;
                _shootOffset = pitchComponent.right*2;

                // Debug.Log($"_shootOffset {_shootOffset}");
                // Debug.Log($"transform.forward {transform.forward}");
                // Debug.Log($"transform.L {transform.up}");
                // Debug.Log($"transform.rotation {transform.rotation}");
                // Debug.Log($"ShootSpeed {ShootSpeed}");

                OnShoot(pitchComponent.position + _shootOffset, pitchComponent.rotation, pitchComponent.right * ShootSpeed, ShooterType);
            }
        }

        // shoot timeout
        if (_shootTimeoutDelta >= 0.0f)
        {
            _shootTimeoutDelta -= Time.deltaTime;
            // Debug.Log($"Shoot timeout {_shootTimeoutDelta}, {Time.deltaTime}");
        }
    }

    [ServerRpc]
    public void ControlRobotServerRpc(float[] input, ServerRpcParams serverRpcParams = default)
    {
        // Debug.Log("Client uploading user input: " + serverRpcParams.Receive.SenderClientId);
        _input = input;

        if (_input[5] > 0)
        {
            isSpin = !isSpin;
        }

        Debug.DrawLine(Base.position, Base.right * 20 + Base.position, Color.blue);
        Debug.DrawLine(yawComponent.position, yawComponent.right * 20 + yawComponent.position, Color.green);
        Debug.DrawLine(pitchComponent.position, pitchComponent.right * 20 + pitchComponent.position, Color.yellow);

        MoveSight();
        Move();
        Shoot();
    }
}
