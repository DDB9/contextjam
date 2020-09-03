using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Initialize the public variables
    public float movementSpeed = 250f;
    public Vector2 jumpForce = new Vector2(50f, 150f);
    public float jumpChargeDuration = 5f;
    public float jumpTiltForce = 100f;

    // Initialize the private variables
    private Rigidbody rb = null;
    private bool isGrounded = true;
    private bool jumpIsCharging = false;
    private float jumpChargeTimer = 0f;
    private float targetJumpForce = 0f;
    private float jumpChargePercentage = 0f;
    private Transform aimTransform = null;
    public bool magneticBootsActive = false;

    private Vector3 targetVector = new Vector3();
    private Vector3 startVector = new Vector3();
    private Quaternion targetRotation = new Quaternion();
    private float impactDistance = 0f;
    private float impactRaycastLength = 6f;

    // Input variables
    private float inputMove = 0f;
    private bool inputJump = false;
    private bool inputMagneticBoots = false;

    public bool JumpIsCharging
    {
        get { return jumpIsCharging; }
    }

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    private void Update()
    {
        GetInput();
        UpdateConstraints();
        UpdateUI();
        UpdateAimTransform();

        if (!isGrounded)
        {
            if (magneticBootsActive)
            {
                AlignToSurface();
            }

            ToggleMagneticBoots();
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
        aimTransform = transform.Find("Aim");
    }

    // Get input from the user
    private void GetInput()
    {
        inputMove = Input.GetAxisRaw("Horizontal");
        inputJump = Input.GetButton("Jump");
        inputMagneticBoots = Input.GetButtonDown("MagneticBoots");
    }

    // Update the rigidbody constraints
    private void UpdateConstraints()
    {
        if (isGrounded)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
        }
    }

    // Move the character horizontally (relative to its orientation)
    private void Move()
    {
        float step = movementSpeed * Time.deltaTime;
        Vector3 newVel = transform.right * (inputMove * step);
        rb.velocity = newVel;
    }

    // Jump away from the current surface
    private void Jump()
    {
        if (inputJump)
        {
            if (!jumpIsCharging)
            {
                jumpChargeTimer = jumpChargeDuration;
                jumpIsCharging = true;
            }

            jumpChargePercentage = 1f - (jumpChargeTimer / jumpChargeDuration);
            targetJumpForce = Mathf.Lerp(jumpForce.x, jumpForce.y, jumpChargePercentage);
            jumpChargeTimer -= Time.deltaTime;
        }

        if ((!inputJump || targetJumpForce == jumpForce.y) && jumpIsCharging)
        {
            isGrounded = false;
            UpdateConstraints();

            float step = targetJumpForce * Time.deltaTime;
            rb.AddForce(aimTransform.right * step, ForceMode.Impulse);

            step = jumpTiltForce * Time.deltaTime;
            Vector3 torque = new Vector3(0f, 0f, (jumpChargePercentage * step) * Mathf.Sign(-aimTransform.right.x));
            rb.AddTorque(torque, ForceMode.Impulse);

            jumpIsCharging = false;
        }
    }

    // Update the UI with the current player data
    private void UpdateUI()
    {
        GameManager.Instance.UiManager.JumpChargeSlider.value = jumpChargePercentage;
    }

    // Update the direction of the aim transform to point towards the mouse cursor
    private void UpdateAimTransform()
    {
        aimTransform.right = (GameManager.Instance._Cursor.transform.position - transform.position).normalized;
    }

    private void AlignToSurface()
    {
        RaycastHit[] hits;

        if (rb.velocity.magnitude > 0)
        {
            hits = Physics.RaycastAll(transform.position - transform.up, rb.velocity.normalized, impactRaycastLength);

            if (hits.Length == 0)
            {
                startVector = transform.up;
            }
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];

                if (hits[i].transform.tag == "Level")
                {
                    targetVector = hits[i].normal;
                    impactDistance = hits[i].distance / impactRaycastLength;
                    Orientation(targetVector, impactDistance, startVector, hits[i].point);
                }
            }
        }
    }

    private void Orientation(Vector3 targetVector, float distance, Vector3 startVector, Vector3 targetPoint)
    {
        transform.up = Vector3.Slerp(startVector, targetVector, (1 - distance));
        float distToTarget = Vector3.Distance(transform.position, targetPoint);
        if (distToTarget <= 1.2f)
        {
            transform.up = targetVector;
            magneticBootsActive = false;
            isGrounded = true;
        }

        Debug.Log(distToTarget);
    }

    private void ToggleMagneticBoots()
    {
        if (inputMagneticBoots)
        {
            magneticBootsActive = !magneticBootsActive;
        }
    }
}
