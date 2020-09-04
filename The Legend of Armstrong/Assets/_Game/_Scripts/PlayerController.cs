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
    public ParticleSystem lightArrowShoot;
    public GameObject jumpDirectionSprite;
    public float jumpChargeShakeIntensity = 1f;
    public SpriteRenderer characterSpriteRenderer;
    public Sprite[] characterDirectionSprites;

    // Initialize the private variables
    private Rigidbody rb = null;
    private CapsuleCollider playerCollider = null;
    private Transform aimTransform = null;
    public Transform bowTransform;
    private bool isGrounded = false;
    private Vector3 surfacePosition = new Vector3();
    private Quaternion surfaceRotation = new Quaternion();
    private Quaternion preJumpRotation = new Quaternion();
    private float distanceToSurface = 0f;
    private List<SpriteRenderer> equipment = new List<SpriteRenderer>();
    private int equipmentId = -1;
    private bool isGrappling = false;
    public int hp = 0;
    private int bombs = 3;
    private float jumpChargeTimer = 0f;
    private bool jumpIsCharging = false;
    private GameObject projectile;
    private float characterDirection;
    private float weaponHandSwapSpeed = 20f;

    // Input
    private float inputMove = 0f;
    private bool inputJump = false;
    private bool inputGrappleUp = false;
    private bool inputShoot = false;
    private bool inputBomb = false;
    private bool inputShield = false;
    private bool inputRelease = false;

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
        SpriteController();

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
        inputShoot = Input.GetButton("Fire1");
        inputRelease = Input.GetButtonUp("Fire1");
        inputBomb = Input.GetButtonDown("Bomb");
        inputShield = Input.GetButton("Shield");
        characterDirection = Vector3.Dot((GameManager.Instance._Cursor.transform.position - transform.position).normalized, transform.right);
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
        Vector3 newVel;
        float step = movementSpeed * Time.deltaTime;
        if (!inputJump)
            newVel = transform.right * (inputMove * step);
        else
            newVel = Vector3.zero;
        rb.velocity = Vector3.Lerp(rb.velocity, newVel, movementSmoothing);

    }

    // Jump towards the mouse cursor
    private void Jump()
    {
        if (inputJump)
        {
            jumpDirectionSprite.SetActive(true);
            if (!jumpIsCharging)
            {
                jumpChargeTimer = jumpChargeDuration;
                jumpIsCharging = true;
                
            }

            float shakeIntensity = jumpChargeShakeIntensity * (1f - (jumpChargeTimer / jumpChargeDuration));
            float newPosX = Random.Range(-shakeIntensity, shakeIntensity);
            float newPosY = Random.Range(-shakeIntensity, shakeIntensity);
            transform.Find("Sprite").localPosition = new Vector3(newPosX, newPosY, 0f);

            jumpChargeTimer = jumpChargeTimer - Time.deltaTime;
        }
        else
        {
            transform.Find("Sprite").localPosition = new Vector3();
            jumpDirectionSprite.SetActive(false);
        }

        if (!inputJump && jumpIsCharging && FindSurface(aimTransform.right))
        {
            preJumpRotation = transform.rotation;

            Vector3 direction = aimTransform.right;
            float percentage = 1f - (jumpChargeTimer / jumpChargeDuration);
            float step = (jumpForce * (0.1f + 0.9f * percentage) * Time.deltaTime);
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
            surfacePosition = hit.point + (dirToPlayer * ((playerCollider.height * transform.localScale.x) / 2f));
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
            float range = ((playerCollider.height * transform.localScale.x) / 2f) * 1.25f;
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
            if (inputShield)
            {
                equipmentId = 3;
            }
            else
            {
                equipmentId = (inputShoot) ? 0 : 2;
            }
        }

        for (int i = 0; i < equipment.Count; i++)
        {
            float sign = Mathf.Sign(Vector3.Dot((GameManager.Instance._Cursor.transform.position - transform.position).normalized, transform.right));
            equipment[i].gameObject.SetActive(i == equipmentId);
            if(characterDirection > 0)
            {
                if(i == 2)
                    equipment[i].gameObject.GetComponent<SpriteRenderer>().flipX = false;
                else
                    equipment[i].gameObject.GetComponent<SpriteRenderer>().flipY = false;     
            }
            else
            {
                if (i == 2)
                    equipment[i].gameObject.GetComponent<SpriteRenderer>().flipX = true;
                else
                    equipment[i].gameObject.GetComponent<SpriteRenderer>().flipY = true;
            }           
            equipment[i].transform.localPosition = new Vector3(equipment[i].transform.localPosition.x, -sign * Mathf.Abs(equipment[i].transform.localPosition.y), equipment[i].transform.localPosition.z);
        }
    }

    private void Shoot()
    {

        if (inputShoot && !inputShield)
        {
            if(projectile == null)
                projectile = Instantiate(projectilePrefab, bowTransform.position, Quaternion.identity, bowTransform);
            projectile.transform.localPosition = Vector3.zero;
            projectile.transform.localRotation = Quaternion.identity;
            projectile.transform.GetChild(2).gameObject.SetActive(false);
        }
        if (inputRelease)
        {
            if(projectile != null)
            {
                projectile.transform.parent = null;
                projectile.GetComponent<Projectile>().ProjectileSpeed = projectileSpeed;
                projectile.GetComponent<Projectile>().released = true;
                projectile.transform.GetChild(2).gameObject.SetActive(true);
                bowDisplayTimer = bowDisplayDuration;
                shootTimer = shootDelay;
                projectile = null;
                lightArrowShoot.Play();
            }
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

    public void SpriteController()
    {
        if(Mathf.Abs(characterDirection) <= 0.4f)
            characterSpriteRenderer.sprite = characterDirectionSprites[0];
        else if(characterDirection > 0.4f)
        {
            characterSpriteRenderer.sprite = characterDirectionSprites[1];
            characterSpriteRenderer.flipX = false;
        }
        else
        {
            characterSpriteRenderer.sprite = characterDirectionSprites[1];
            characterSpriteRenderer.flipX = true;
        }

    }
}
