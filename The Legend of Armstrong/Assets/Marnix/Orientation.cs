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
                    color = Color.green;
                    targetVector = hits[i].normal;
                    impactDistance = hits[i].distance / impactRaycastLength;    
                    Debug.Log(impactDistance);
                    Orientatie(targetVector, impactDistance, startVector);
                    Debug.DrawLine(hits[i].point, hits[i].point + hits[i].normal * 2f, Color.blue);
                }
                else
                {
                    color = Color.red;
                    
                }
            }
            Debug.DrawLine(transform.position - transform.up, transform.position - transform.up + rb.velocity.normalized * impactRaycastLength, color);

        }
    }

    private void Orientatie(Vector3 targetVector, float distance, Vector3 startVector)
    {
        transform.up = Vector3.Slerp(startVector, targetVector, (1 - distance));

        //targetRotation = Quaternion.LookRotation(targetVector, Vector3.forward);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1 - impactDistance / impactRaycastLength);
    }



}
