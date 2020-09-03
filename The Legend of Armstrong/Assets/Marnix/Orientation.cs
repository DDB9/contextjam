using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orientation : MonoBehaviour
{
    private Rigidbody rb;
    private Color color;
    private Vector3 targetVector;
    private Vector3 startVector;
    private Quaternion targetRotation;
    private float impactDistance;
    private float impactRaycastLength = 4f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(-0.5f, -0.5f, 0);
        rb.angularVelocity = new Vector3(0, 0, 0.4f);
        color = Color.red;        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Orientatie(Vector3 targetVector, float distance, Vector3 startVector)
    {
        transform.up = Vector3.Slerp(startVector, targetVector, (1 - distance));

        //targetRotation = Quaternion.LookRotation(targetVector, Vector3.forward);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1 - impactDistance / impactRaycastLength);
    }



}
