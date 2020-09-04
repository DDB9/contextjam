using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IDamageable<int>
{
    // Initialize the public variables
    public float movementSpeed = 250f;
    public float movementSmoothing = .1f;
    public float jumpForce = 100f;
    public float jumpChargeDuration = 1f;
    public float groundCheckDelay = .5f;
    public float shootDelay = 1f;
    public float projectileSpeed = 1f;
    public float bowDisplayDuration = 1f;
    public int maxHp = 3;
    public float bombSpeed = 1f;
    public GameObject projectilePrefab = null;
    public GameObject bombPrefab = null;
    public float jumpChargeShakeIntensity = 1f;

    // Initialize the private variables
    private Rigidbody rb = null;
    private CapsuleCollider playerCollider = null;
    private Transform aimTransform = null;
    private bool isGrounded = false;
    private Vector3 surfacePosition = new Vector3();
    private Quaternion surfaceRotation = new Quaternion();
    private Quaternion preJumpRotation = new Quaternion();
    private float distanceToSurface = 0f;
    private List<SpriteRenderer> equipment = new List<SpriteRenderer>();
    private int equipmentId = -1;
    private bool isGrappling = false;
    private int hp = 0;
    private int bombs = 3;
    private float jumpChargeTimer = 0f;
    private bool jumpIsCharging = false;

    // Input
    private float inputMove = 0f;
    private bool inputJump = false;
    private bool inputGrapple = false;
    private bool inputGrappleUp = false;
    private bool inputShoot = false;
    private bool inputBomb = false;

    // Timers
    private float groundCheckTimer = 0f;
    private float shootTimer = 0f;
    private float bowDisplayTimer = 0f;

    public bool IsGrappling
    {
        set { isGrappling = value; }
    }

    public int Hp
    {
        get { return hp; }
    }

    public int Bombs
    {
        get { return bombs; }
        set { bombs = value; }
    }

    public float JumpChargeTimer
    {
        get { return jumpChargeTimer; }
    }
    
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
        Aim();
        CheckIfGrounded();
        UpdateConstraints();
        ReleaseGrapple();
        ToggleEquipment();
        Bomb();

        if (!isGrounded)
        {
            AlignToSurface();
        }

        if (!isGrappling)
        {
            Shoot();
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

        hp = maxHp;

        foreach (Transform child in transform.Find("Aim").transform.Find("Equipment"))
        {
            equipment.Add(child.GetComponent<SpriteRenderer>());
        }
    }

    // Get input from the user
    private void GetInput()
    {
        inputMove = Input.GetAxisRaw("Horizontal");
        inputJump = Input.GetButton("Jump");
        inputGrappleUp = Input.GetButtonUp("Fire2");
        inputShoot = Input.GetButtonDown("Fire1");
        inputBomb = Input.GetButtonDown("Bomb");
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
        if (inputJump)
        {
            if (!jumpIsCharging)
            {
                jumpChargeTimer = jumpChargeDuration;
                jumpIsCharging = true;
            }

            jumpChargeTimer = Mathf.Clamp(jumpChargeTimer - Time.deltaTime, 0f, 1f);
        }

        if (!inputJump && jumpIsCharging && FindSurface(aimTransform.right))
        {
            preJumpRotation = transform.rotation;

            Vector3 direction = aimTransform.right;
            float percentage = 1f - (jumpChargeTimer / jumpChargeDuration);
            float step = (jumpForce * percentage) * Time.deltaTime;
            rb.velocity = (direction * step);

            groundCheckTimer = groundCheckDelay;
            jumpIsCharging = false;
        }
    }

    // Release the grapple hook
    private void ReleaseGrapple()
    {
        if (inputGrappleUp && FindSurface(rb.velocity.normalized))
        {
            preJumpRotation = transform.rotation;
            groundCheckTimer = groundCheckDelay;
        }
    }

    // Find a surface to jump to (return true if one is found)
    private bool FindSurface(Vector3 direction)
    {
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
            float range = (playerCollider.height / 2f) * 1.25f;
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

    private void ToggleEquipment()
    {
        if (isGrappling)
        {
            equipmentId = 1;
            bowDisplayTimer = 0f;
        }
        else
        {
            equipmentId = (bowDisplayTimer > 0f) ? 0 : -1;
        }

        for (int i = 0; i < equipment.Count; i++)
        {
            equipment[i].enabled = (i == equipmentId);
        }
    }

    private void Shoot()
    {
        if (inputShoot && shootTimer <= 0f)
        {
            GameObject projectile = Instantiate(projectilePrefab, rb.position, Quaternion.identity);
            projectile.transform.right = aimTransform.right;
            projectile.GetComponent<Projectile>().ProjectileSpeed = projectileSpeed;

            bowDisplayTimer = bowDisplayDuration;
            shootTimer = shootDelay;
        }

        bowDisplayTimer -= Time.deltaTime;
        shootTimer -= Time.deltaTime;
    }

    private void Die()
    {
        Debug.Log("You died");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Bomb()
    {
        if (inputBomb && bombs > 0)
        {
            GameObject bomb = Instantiate(bombPrefab, rb.position, Quaternion.identity);
            bomb.GetComponent<Bomb>().Speed = bombSpeed;
            bomb.transform.right = aimTransform.right;

            bombs--;
        }
    }

    public void RecalculatePath()
    {
        if (FindSurface(rb.velocity.normalized) && (rb.velocity.magnitude > 0f) && !isGrounded)
        {
            preJumpRotation = transform.rotation;
            groundCheckTimer = groundCheckDelay;
        }
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;

        if (hp <= 0)
        {
            Die();
        }
    }

    public void Blast(Vector3 direction, float force)
    {
        preJumpRotation = transform.rotation;

        float step = force * Time.deltaTime;
        rb.velocity = (direction * step);

        groundCheckTimer = groundCheckDelay;
    }
}
