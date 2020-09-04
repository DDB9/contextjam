using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Initialize the private variables
    private Rigidbody rb = null;
    private float projectileSpeed = 0f;
    private int damage = 1;
    public bool isHostile = false;

    public float ProjectileSpeed
    {
        set { projectileSpeed = value; }
    }

    public int Damage
    {
        set { damage = value; }
    }

    public bool IsHostile
    {
        set { isHostile = value; }
    }

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable<int> damageable = other.GetComponentInParent<IDamageable<int>>();
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (damageable != null)
        {
            if ((isHostile && player != null) || (!isHostile && player == null))
            {
                damageable.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Initialize the projectile
    private void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.right * projectileSpeed;
    }
}
