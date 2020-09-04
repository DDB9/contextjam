using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    public GameObject hookHit;
    public GameObject hook;
    public GameObject chain;
    public ParticleSystem PS_GrappleShoot;
    public ParticleSystem PS_GrappleHit;
    public GameObject player;
    public Camera cam;
    public Transform hookShotPos;
    public Rigidbody rb;
    private LineRenderer lineRend;
    public Transform equipmentPos;

    public LayerMask mask;
    public bool useMouseAim;
    public float maxGrappleDistance;
    public float jointSpringValue = 4.5f;
    public float jointDamperValue = 7f;
    public float jointMassScaleValue = 4.5f;

    private float angle;
    private Vector3 grappleHitPoint;
    private Vector2 mousePos;
    private Vector2 newMousePos;
    private Transform aimTransform;
    private SpringJoint joint;
    
    // Start is called before the first frame update
    void Start()
    {
        aimTransform = transform.Find("Aim");
        lineRend = GetComponent<LineRenderer>();
        hookHit.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        newMousePos = new Vector2(rb.position.x, rb.position.y);

        if (Input.GetMouseButtonDown(1))
        {
            GrappleHookPerform();
            GameManager.Instance.Player.IsGrappling = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            GrappleHookStop();
            GameManager.Instance.Player.IsGrappling = false;
        }
    }

    void FixedUpdate()
    {
        Vector2 LookDir = mousePos - newMousePos;
        angle = Mathf.Atan2(LookDir.y,LookDir.x) * Mathf.Rad2Deg - 90f;
        if(useMouseAim)
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    void LateUpdate()
    {
        DrawRope();
    }

    void DrawRope()
    {
        if (!joint) return;

        lineRend.SetPosition(0, hookShotPos.position);
        lineRend.SetPosition(1, grappleHitPoint);
    }

    void GrappleHookPerform()
    {
        RaycastHit hit;
        if(Physics.Raycast(hookShotPos.position, hookShotPos.up, out hit, Mathf.Infinity, mask))
        {
            hook.SetActive(false);
            hookHit.SetActive(true);
            chain.SetActive(false);

            PS_GrappleShoot.Play();
            PS_GrappleHit.Play();

            grappleHitPoint = hit.point;
            joint = player.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grappleHitPoint;

            float distanceFromPoint = Vector3.Distance(hookShotPos.position, grappleHitPoint);

            joint.maxDistance = distanceFromPoint * .6f;
            joint.minDistance = distanceFromPoint * .30f;

            joint.spring = jointSpringValue;
            joint.damper = jointDamperValue;
            joint.massScale = jointMassScaleValue;

            lineRend.positionCount = 2;
            hookHit.transform.position = hit.point;
            hookHit.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }

    void GrappleHookStop()
    {
        hook.SetActive(true);
        hookHit.SetActive(false);
        chain.SetActive(true);

        lineRend.positionCount = 0;
        Destroy(joint);
    }
}
