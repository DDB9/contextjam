using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    // Initialize the public variables
    public float explodeDuration = 3f;
    public GameObject explosionPrefab = null;
    public float shakeIntensity = 1f;

    // Initialize the private variables
    private float explodeTimer = 0f;
    private float speed = 1f;
    private int damage = 1;

    public float Speed
    {
        set { speed = value; }
    }

    public int Damage
    {
        set { damage = value; }
    }

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    private void Update()
    {
        Shake();
        Explode();
    }

    // Initialize the bomb
    private void Initialize()
    {
        explodeTimer = explodeDuration;
        GetComponent<Rigidbody>().velocity = transform.right * speed;
    }

    private void Shake()
    {
        float percentage = 1f - (explodeTimer / explodeDuration);
        float intensity = Mathf.Lerp(0f, shakeIntensity, percentage);

        float newPosX = Random.Range(-intensity, intensity);
        float newPosY = Random.Range(-intensity, intensity);
        transform.Find("Sprite").localPosition = new Vector3(newPosX, newPosY, 0f);
    }

    // Explode
    private void Explode()
    {
        if (explodeTimer <= 0f)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.GetComponent<Explosion>().Damage = damage;

            Destroy(gameObject);
        }
        else
        {
            explodeTimer -= Time.deltaTime;
        }
    }
}
