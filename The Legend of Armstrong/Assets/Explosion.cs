using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float force = 500f;

    private float destroyTimer = 1f;
    private int damage = 0;

    public int Damage
    {
        set { damage = value; }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        IDamageable<int> damageable = other.GetComponentInParent<IDamageable<int>>();
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (damageable != null)
        {
            if (player == null)
            {
                damageable.TakeDamage(damage);
            }
            else
            {
                Vector3 direction = (player.transform.position - transform.position).normalized;
                player.Blast(direction, force);
            }
        }

        Destructable destructable = other.GetComponent<Destructable>();
        if (destructable != null)
        {
            Destroy(other.gameObject);
        }

        Destroy(gameObject);
    }
}
