using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // Initialize the public variables
    public float movementSpeed = 250f;
    public float movementSmoothing = .1f;
    public float jumpForce = 100f;
    public float groundCheckDelay = .5f;

    // Initialize the private variables
    private Rigidbody rb = null;
    private CapsuleCollider playerCollider = null;
    private Transform aimTransform = null;
    private bool isGrounded = false;
    Vector3 surfacePosition = new Vector3();
    Quaternion surfaceRotation = new Quaternion();
    Quaternion preJumpRotation = new Quaternion();
    float distanceToSurface = 0f;

    // Input
    private float inputMove = 0f;
    private bool inputJump = false;

    // Timers
    private float groundCheckTimer = 0f;

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    private void Update()
    {
        GetInput();
        Aim();
        CheckIfGrounded();
        UpdateConstraints();

        if (!isGrounded)
        {
            AlignToSurface();
        }
    }

    // FixedUpdate is called once per fixed frame
    private void FixedUpdate()
    {
        if (isGrounded)
        {
            Move();
            Jump();
        }
    }

    // Initialize the player controller
    private void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponentInChildren<CapsuleCollider>();
        aimTransform = transform.Find("Aim");
    }

    // Get input from the user
    private void GetInput()
    {
        inputMove = Input.GetAxisRaw("Horizontal");
        inputJump = Input.GetButton("Jump");
    }

    // Rotate the aim transform to point towards the mouse cursor
    private void Aim()
    {
        Vector3 direction = (GameManager.Instance._Cursor.transform.position - transform.position).normalized;
        aimTransform.right = direction;
    }

    // Move the player controller along the current surface
    private void Move()
    {
        float step = movementSpeed * Time.deltaTime;
        Vector3 newVel = transform.right * (inputMove * step);
        rb.velocity = Vector3.Lerp(rb.velocity, newVel, movementSmoothing);
    }

    // Jump towards the mouse cursor
    private void Jump()
    {
        if (inputJump && FindSurface())
        {
            preJumpRotation = transform.rotation;

            Vector3 direction = aimTransform.right;
            float step = jumpForce * Time.deltaTime;
            rb.velocity = (direction * step);

            groundCheckTimer = groundCheckDelay;
        }
    }

    // Find a surface to jump to (return true if one is found)
    private bool FindSurface()
    {
        Vector3 direction = aimTransform.right;
        RaycastHit hit = new RaycastHit();
        LayerMask mask = LayerMask.GetMask("Solid");
        bool hitSurface = (Physics.Raycast(rb.position, direction, out hit, Mathf.Infinity, mask));

        if (hitSurface)
        {
            Vector3 dirToPlayer = (rb.position - hit.point).normalized;
            surfacePosition = hit.point + (dirToPlayer * (playerCollider.height / 2f));
            surfaceRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            distanceToSurface = Vector3.Distance(rb.position, surfacePosition);

            float angle = Quaternion.Angle(transform.rotation, surfaceRotation);
            if (angle == 180f)
            {
                surfaceRotation = Quaternion.Euler(0f, 0f, transform.eulerAngles.z + 180f);
            }
        }

        return hitSurface;
    }

    // Align the player to the next surface
    private void AlignToSurface()
    {
        float distance = Vector3.Distance(rb.position, surfacePosition);
        float percentage = Mathf.Clamp(1f - (distance / distanceToSurface), 0f, 1f);
        transform.rotation = Quaternion.Slerp(preJumpRotation, surfaceRotation, percentage);
    }

    // Check if the player is currently grounded
    private void CheckIfGrounded()
    {
        if (groundCheckTimer <= 0f)
        {
            Vector3 direction = -transform.up;
            float range = (playerCollider.height / 2f) * 1.1f;
            LayerMask mask = LayerMask.GetMask("Solid");
            bool hitSurface = (Physics.Raycast(rb.position, direction, range, mask));

            if (hitSurface)
            {
                transform.rotation = surfaceRotation;
            }

            isGrounded = hitSurface;
        }
        else
        {
            isGrounded = false;
            groundCheckTimer -= Time.deltaTime;
        }
    }

    // Update the rigidbody constraints
    private void UpdateConstraints()
    {
        if (isGrounded)
        {
            rb.constraints = (RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ);
        }
        else
        {
            rb.constraints = (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ);
        }
    }
}
