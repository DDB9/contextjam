using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float initializationTime;
    private void Start()
    {
        initializationTime = Time.timeSinceLevelLoad;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        transform.LookAt(GameObject.FindGameObjectWithTag("Player").transform.position);
        transform.Rotate(new Vector3(0, -90, 0), Space.Self);

        float _timeSinceInitialization = Time.timeSinceLevelLoad - initializationTime;

        if (_timeSinceInitialization > 5f) Destroy(gameObject);
    }
}
