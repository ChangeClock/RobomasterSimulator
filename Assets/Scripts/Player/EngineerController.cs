using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.Networking;
using Cinemachine;

public class EngineerController : NetworkBehaviour
{
    public bool Enabled = false;

    // private PlayerInputs _input;
    private float[] _input = new float[6];
    [SerializeField] private InputAction Boost;
    [SerializeField] private float BoostPower = 240;

    [Header("Chassis")]
    private Transform Base;

    private PIDController moveController;
    private PIDController followController;
    [SerializeField] private float[] moveControllerParameters = {0.02f, 0f, 0.015f};
    [SerializeField] private float[] followControllerParameters = {0.02f, 0f, 0.015f};
    
    private Transform[] wheels = new Transform[4];
    private float[] wheelForce = new float[4];

    [SerializeField] private bool isSpin = false;

    public override void OnNetworkSpawn()
    {
        // Debug.Log("Client:" + NetworkManager.Singleton.LocalClientId + "IsOwner?" + IsOwner);
        if (IsOwner) {
            this.gameObject.GetComponent<PlayerInput>().enabled = true;
        }
    }

    void Start()
    {
        Base = transform.Find("Base");

        moveController = new PIDController(moveControllerParameters[0],
                                            moveControllerParameters[1],
                                            moveControllerParameters[2]);

        followController = new PIDController(followControllerParameters[0],
                                            followControllerParameters[1],
                                            followControllerParameters[2]);

        wheels[0] = transform.Find("Right-Front-Wheel");
        wheels[1] = transform.Find("Left-Front-Wheel");
        wheels[2] = transform.Find("Left-Back-Wheel");
        wheels[3] = transform.Find("Right-Back-Wheel");
    }

    void OnEnable()
    {
        Boost.Enable();
    }

    void OnDisable()
    {
        Boost.Disable();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (!Enabled) return;

        StarterAssetsInputs _playerInputs = GetComponent<StarterAssetsInputs>();

        _input[0] = _playerInputs.move.x;
        _input[1] = _playerInputs.move.y;
        _input[2] = _playerInputs.look.x;
        _input[3] = _playerInputs.look.y;
        _input[4] = _playerInputs.shoot ? 1f : 0f;
        _input[5] = _playerInputs.spin ? 1f : 0f;

        // Debug.Log($"isShoot ? {_input[4]}");

        _playerInputs.shoot = false;
        _playerInputs.spin = false;

        // All input status
        // Debug.Log(_input[0] + " " + _input[1] + " " + _input[2] + " " + _input[3] + " " + _input[4] + " " + _input[5]);

        ControlRobotServerRpc(_input);
    }

    private void MoveSight()
    {
        
    }

    private void Move()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void ControlRobotServerRpc(float[] input, ServerRpcParams serverRpcParams = default)
    {
        // Debug.Log("Client uploading user input: " + serverRpcParams.Receive.SenderClientId);
        _input = input;

        if (_input[5] > 0)
        {
            isSpin = !isSpin;
        }

        MoveSight();
        Move();
    }
}
