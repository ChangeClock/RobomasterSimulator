using UnityEngine;

public class CameraController : MonoBehaviour 
{
    public delegate void TargetDetectAction(int robotID, Faction faction, Vector3 position);
    public event TargetDetectAction OnTargetDetected;

    private Camera cam;
    private RenderTexture rt;

    [SerializeField] private float DetectRange = 80f;
    [SerializeField] private bool printLog = false;

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
                    // Raise event with the target ID & the target position
                    int id = 0;
                    Faction faction = Faction.Neu;
                    // RefereeController referee = armor.GetComponentInParent<RefereeController>();

                    if (referee != null)
                    {
                        id = referee.RobotID.Value;
                        faction = referee.faction.Value;
                    }

                    if (OnTargetDetected != null) OnTargetDetected(id, faction, armor.transform.position);
                }
            }
        }
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
            Debug.DrawLine(c.transform.position + Vector3.forward * c.nearClipPlane, point, Color.cyan);
            
            Debug.DrawLine(c.transform.position + Vector3.forward * c.nearClipPlane, hit.point, Color.green);
            // check it the first object in the line is the target or its child
            if (hit.transform.gameObject == target || hit.transform.IsChildOf(target.transform))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

}