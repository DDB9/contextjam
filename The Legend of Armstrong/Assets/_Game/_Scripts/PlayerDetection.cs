using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        Enemy _parent = transform.parent.GetComponent<Enemy>();
        if (other.CompareTag("Player"))
        {
            _parent.alerted = true;
            _parent.EnemyAnimator.Play("EnemyMove");

            _parent.transform.LookAt(other.transform.position);
            _parent.transform.Rotate(new Vector3(0, 90, 0), Space.Self);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            transform.parent.GetComponent<Enemy>().alerted = false;
            transform.parent.GetComponent<Enemy>().returnToWaypoint = true;
        }
    }
}
