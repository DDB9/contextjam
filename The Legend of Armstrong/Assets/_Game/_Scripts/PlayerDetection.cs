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
            if (_parent.enemyType != EnemyType.RANGED) _parent.EnemyAnimator.Play("EnemyMove");

            _parent.transform.LookAt(other.transform.position);
            _parent.transform.Rotate(new Vector3(0, 90, 0), Space.Self);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Enemy _parent = transform.parent.GetComponent<Enemy>();
        if (other.CompareTag("Player"))
        {
            _parent.alerted = false;
            _parent.returnToWaypoint = true;

            if (_parent.enemyType == EnemyType.RANGED)
                _parent.invokeRepeatingOnce = false;
        }
    }
}
