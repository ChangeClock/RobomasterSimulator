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

    private PIDController moveController;
    private PIDController followController;
    [SerializeField] private float[] moveControllerParameters = {0.02f, 0f, 0.015f};
    [SerializeField] private float[] followControllerParameters = {0.02f, 0f, 0.015f};
    
    private bool[] wheelsIsGrounded = new bool[4];
    private Transform[] wheels = new Transform[4];
    private float[] wheelForce = new float[4];
    private Vector3[] wheelForceDirection = new Vector3[4];

    [SerializeField] private bool isSpin = false;

    [Header("Gimbal")]
    private PIDController yawController;
    private PIDController pitchController;
    [SerializeField] private float[] yawControllerParameters = {80f, 0f, 0.015f};
    [SerializeField] private float[] pitchControllerParameters = {50f, 0f, 0.015f};
    [SerializeField] private float yawTargetAngle = 0;
    [SerializeField] private float pitchTargetAngle = 0;
    private Transform yawComponent;
    private Transform pitchComponent;
    private HingeJoint yawJoint;
    private HingeJoint pitchJoint;
    private JointMotor yawMotor;
    private JointMotor pitchMotor;
    [SerializeField] private float pitchMax;
    [SerializeField] private float pitchMin;

    [Header("Player Shooter")]
    [SerializeField]private ShooterController Shooter0;
    [SerializeField]private ShooterController Shooter1;

    [Tooltip("Shoot Frequency in HZ")]
    [SerializeField]private float ShootFrequency = 20f;
    [SerializeField]private float ShootSpeed = 30f;

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

        yawController.updatePara(yawControllerParameters[0],
                                yawControllerParameters[1],
                                yawControllerParameters[2]);

        pitchController.updatePara(pitchControllerParameters[0],
                                pitchControllerParameters[1],
                                pitchControllerParameters[2]);

        yawTargetAngle += _input[2];
        if (yawTargetAngle < 0) yawTargetAngle += 360;
        if (yawTargetAngle > 360) yawTargetAngle -= 360;
        pitchTargetAngle += _input[3];
        if (pitchTargetAngle < pitchMin) pitchTargetAngle = pitchMin;
        if (pitchTargetAngle > pitchMax) pitchTargetAngle = pitchMax;

        float _yawDifference = yawTargetAngle - yawComponent.eulerAngles.y;
        float _pitchDifference = pitchTargetAngle - pitchComponent.eulerAngles.z;

        // Debug.Log("yaw: " + yawComponent.eulerAngles.y + " yawTargetAngle: " + yawTargetAngle + "_yawDifference: " + _yawDifference);
        // Debug.Log("pitch: " + pitchComponent.eulerAngles.z + " pitchTargetAngle: " + pitchTargetAngle + " _pitchDifference: " + _pitchDifference);

        // pitchMotor.targetVelocity = -_input[3] * rotateSpeed;

        if (_yawDifference > 180) {
            yawMotor.targetVelocity = yawController.Update(_yawDifference - 360, Time.deltaTime);
        } else if (_yawDifference < -180) {
            yawMotor.targetVelocity = yawController.Update(360 + _yawDifference, Time.deltaTime);
        } else {
            yawMotor.targetVelocity = yawController.Update(_yawDifference, Time.deltaTime);
        }

        pitchMotor.targetVelocity = pitchController.Update(_pitchDifference, Time.deltaTime);

        // Debug.Log("Velocity: " + yawMotor.targetVelocity);
        
        pitchJoint.motor = pitchMotor;
        yawJoint.motor = yawMotor;
    }

    private void Move()
    {
        moveController.updatePara(moveControllerParameters[0],
                                    moveControllerParameters[1],
                                    moveControllerParameters[2]);

        followController.updatePara(followControllerParameters[0],
                                    followControllerParameters[1],
                                    followControllerParameters[2]);

        // Compute the vx & vy according to the YAW direction, and project these vector on base direction to calculate the force should apply to vx and vy on basement.
        Vector3 _inputX = _input[1] * Vector3.Project(yawComponent.right, Base.right) + _input[0] * Vector3.Project(-yawComponent.forward, Base.right);
        Vector3 _inputY = _input[1] * Vector3.Project(yawComponent.right, -Base.forward) + _input[0] * Vector3.Project(-yawComponent.forward, -Base.forward);

        float vx = Vector3.Dot(_inputX.normalized, Base.right) * (_inputX).magnitude;
        float vy = Vector3.Dot(_inputY.normalized, -Base.forward) * (_inputY).magnitude;
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

        wheelForce[0] = vx-vy-vw;
        wheelForce[1] = -vx-vy-vw;
        wheelForce[2] = -vx+vy-vw;
        wheelForce[3] = vx+vy-vw;

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

    private void TriggerShot()
    {
        // TODO: User input to control the shooterController

        if (_input[4] > 0 && _shootTimeoutDelta <= 0.0f) 
        {
            // reset the shoot timeout timer
            _shootTimeoutDelta = 1 / ShootFrequency;

            // Debug.Log("[RobotController] TriggerShot");
            if (Shooter0 != null){
                Shooter0.PullTrigger(ShootSpeed);
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
        Debug.DrawLine(pitchComponent.position, - pitchComponent.right * 20 + pitchComponent.position, Color.yellow);

        MoveSight();
        Move();
        TriggerShot();
    }
}
