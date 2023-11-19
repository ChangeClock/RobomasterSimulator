using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CameraCommunication;

namespace CameraCommunication
{
    public class TargetInfo
    {
        public int ID;
        public Faction Faction;
        public Vector3 Position;
        public float LastTime;
        public float LiveTime = 1.0f;

        public bool IsValid
        {
            get { return LastTime > 0; }
        }

        public TargetInfo(int id, Faction faction, Vector3 position)
        {
            ID = id;
            Faction = faction;
            Position = position;
            LastTime = LiveTime;
        }
    }
}

public class CameraController : MonoBehaviour 
{
    public delegate void TargetDetectAction(List<TargetInfo> targets);
    public event TargetDetectAction OnTargetDetected;

    private Camera cam;
    private RenderTexture rt;

    [SerializeField] private float DetectRange = 80f;
    [SerializeField] private bool printLog = false;

    [SerializeField] private Vector3 camOffset = new Vector3(0f, 0f, 0f);

    List<TargetInfo> targets = new List<TargetInfo>();

    void Start() 
    {
        rt = new RenderTexture(640, 480, 32);
        cam = GetComponent<Camera>();
        cam.targetTexture = rt;
        cam.enabled = true;
    }

    void Update()
    {
        // Find all game object with tag "Player" within 8 meters of the camera
        Collider[] Armors = Physics.OverlapSphere(transform.position, DetectRange, 1 << LayerMask.NameToLayer("Armor"));
        // if (printLog) Debug.Log($"[CameraController] {Armors.Length} armors detected");

        targets.Clear();

        foreach (var armor in Armors)
        {
            RefereeController referee = armor.GetComponentInParent<RefereeController>();

            if (armor.GetComponent<ArmorController>() != null)
            {
                ArmorController armorController = armor.GetComponent<ArmorController>();
                if (armorController.Enabled == false) continue;
                
                // if (printLog) Debug.Log($"[CameraController] {referee.RobotID.Value} detected {IsVisible(cam, armor.gameObject)}");

                // Check if the target is visible
                if (IsVisible(cam, armor.gameObject))
                {
                    if (printLog) Debug.Log($"[CameraController] {armor.gameObject.name} detected, offset {GetRotateOffset(armor.transform.position).magnitude}");
                    // Raise event with the target ID & the target position
                    if (referee != null)
                    {
                        targets.Add(new TargetInfo(referee.RobotID.Value, referee.faction.Value, armor.transform.position));
                    }
                }
            }
        }

        // if (printLog) Debug.DrawLine(transform.position, position, Color.red);
        if (OnTargetDetected != null) OnTargetDetected(targets);
    }

    private bool IsVisible(Camera c, GameObject target)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(c);
        var point = target.transform.position;

        foreach (var plane in planes)
        {
            if (plane.GetDistanceToPoint(point)< 0)
            {
                return false;
            }
        }

        // Check if the target is blocked by other objects
        RaycastHit hit;

        // Visualize the line
        if (Physics.Linecast(c.transform.position + Vector3.forward * c.nearClipPlane, point, out hit))
        {
            // Debug.DrawLine(c.transform.position + Vector3.forward * c.nearClipPlane, point, Color.cyan);
            
            // if (printLog) Debug.DrawLine(c.transform.position + Vector3.forward * c.nearClipPlane, hit.point, Color.green);
            // check it the first object in the line is the target or its child
            if (hit.collider.gameObject == target)
            {
                if (printLog) Debug.DrawLine(c.transform.position + Vector3.forward * c.nearClipPlane, hit.point, Color.yellow);
                // if (printLog) Debug.Log($"[CameraController] {hit.collider.gameObject.name} detected");
                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    bool CompareTargetOffset(Vector3 target1, Vector3 target2)
    {
        return GetRotateOffset(target1).magnitude < GetRotateOffset(target2).magnitude;
    }

    Vector2 GetRotateOffset(Vector3 target)
    {
        Vector3 offset = target - transform.position;
        return new Vector2(offset.x, offset.z);
    }
}