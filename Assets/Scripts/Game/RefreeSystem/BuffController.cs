using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BuffController : RefereeController 
{
    [SerializeField] private HingeJoint Hinge;

    public NetworkVariable<BuffType> Type = new NetworkVariable<BuffType>(BuffType.Small);
    public NetworkList<int> Scores = new NetworkList<int>();
    [SerializeField] private List<TargetController> Targets = new List<TargetController>();

    public NetworkVariable<float> IdelTime            = new NetworkVariable<float>(0);
    public NetworkVariable<float> TimeoutThreshold            = new NetworkVariable<float>(2.5f);
    public NetworkVariable<int> NextTargetID            = new NetworkVariable<int>(0);

    [SerializeField] private float SpinTime = 0;
    [SerializeField] private float[] faRange = {0.780f, 1.045f};
    private float fa = 0.780f;
    [SerializeField] private float[] fwRange = {1.884f, 2.000f};
    private float fw = 1.884f;

    private int SpinDirection = 1;

    public delegate void ActiveAction(Faction faction, BuffType type, int totalScore);
    public static event ActiveAction OnActive;

    public override void OnNetworkSpawn()
    {
        if (IsServer) Hinge.useMotor = true;
    }

    protected override void Start()
    {
        base.Start();

        foreach(var target in Targets)
        {
            Scores.Add(target.Score);
        }
    } 

    protected override void OnEnable()
    {
        foreach(var target in Targets)
        {
            target.OnScore += HitHandler;
        }
    }

    protected override void OnDisable()
    {
        foreach(var target in Targets)
        {
            target.OnScore -= HitHandler;
        }
    }

    protected override void Update()
    {
        if (!IsServer) return;

        var motor = Hinge.motor;

        if (!Enabled.Value)
        {
            motor.targetVelocity = 0;
            Hinge.motor = motor;
            return;
        } else {
            switch(Type.Value)
            {
                case BuffType.Small:
                    motor.targetVelocity = SpinDirection * 60;
                    break;
                case BuffType.Big:
                    motor.targetVelocity = SpinDirection * (fa * Mathf.Sin(fw * SpinTime) + 2.09f - fa) * 60;
                    break;
                default:
                    break;
            }
            Hinge.motor = motor;
            SpinTime += Time.deltaTime;
        }

        IdelTime.Value += Time.deltaTime;
        if (IdelTime.Value > TimeoutThreshold.Value)
        {
            Reset();
        }

        foreach (var target in Targets)
        {
            if (target.TargetID == NextTargetID.Value)
            {
                target.IsTarget = true;
            } else {
                target.IsTarget = false;
            }
        }
    }

    public void HitHandler(int id, int score = 1)
    {
        if (!IsServer) return;

        if (score <= 0) return;

        Debug.Log($"[BuffController] HitHandler {id} {score}");

        if (id == NextTargetID.Value) 
        {
            foreach (var target in Targets)
            {
                if (target.TargetID == id)
                {
                    target.IsActive = true;
                    target.Score = score;
                    Scores.Add(target.Score);
                }
            }

            if (Scores.Count >= Targets.Count)
            {
                int totalScore = 0;
                foreach(var item in Scores)
                {
                    totalScore += item;
                }
                
                Reset();
                OnActive(faction.Value, Type.Value, totalScore);
            } else {
                GetNewTarget();
                IdelTime.Value = 0;
            }
        }
        else
        {
            Reset();
        }
    }

    public override void Reset()
    {
        Scores.Clear();
        foreach(var target in Targets)
        {
            target.Reset();
        }

        IdelTime.Value = 0;

        GetNewTarget();
        // Debug.Log($"[BuffController] NextTarget {NextTargetID.Value}");
    }

    public void Toggle(bool enable, BuffType type = BuffType.Small)
    {
        Enabled.Value = enable;

        foreach(var target in Targets)
        {
            target.Reset();
            // target.Enabled = enable;
        }

        SpinTime = 0;
        fa = Random.Range(faRange[0], faRange[1]);
        fw = Random.Range(fwRange[0], fwRange[1]);

        SpinDirection = (Random.Range(-1,1) < 0) ? -1 : 1;

        if (enable) Reset();
    }

    void GetNewTarget()
    {
        if (Scores.Count > Targets.Count) return;

        int newTarget = Random.Range(0, Targets.Count);
        while (newTarget == NextTargetID.Value || newTarget > Targets.Count || Targets[newTarget].IsActive)
        {
            newTarget = Random.Range(0, Targets.Count);
        }
        NextTargetID.Value = newTarget;
    }
}