using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType {STATIC, FLYING, RANGED};
[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    public EnemyType enemyType;
    public Animator EnemyAnimator;
    public GameObject player;
    public GameObject projectile;
    public Transform[] waypoints;
    public float movementSpeed;
    public bool alerted;
    public bool disablePatrol;
    public bool ranged;

    [HideInInspector]
    public Transform targetWaypoint;
    [HideInInspector]
    public bool returnToWaypoint;

    private bool _patrolDirectionRight;

    private void Start()
    {
        targetWaypoint = waypoints[0];
    }

    private void Update()
    {
        switch (enemyType)
        {
            case EnemyType.STATIC:
                if (alerted) ChasePlayer();
                else if (returnToWaypoint) ReturnToWaypoint();
                break;

            case EnemyType.FLYING:
                Patrol();
                break;

            case EnemyType.RANGED:
                if (alerted) Invoke("FireBullet", 0.1f);
                else CancelInvoke();
                break;
        }
    }

    private void Patrol()
    {
        // Switch waypoints if enemy has reached a waypoint
        if (!_patrolDirectionRight && transform.position.x <= targetWaypoint.position.x) SwitchWaypoint();
        else if (_patrolDirectionRight && transform.position.x >= targetWaypoint.position.x) SwitchWaypoint();

        // Move enemy to target waypoint.
        transform.LookAt(targetWaypoint.position);
        transform.Rotate(new Vector3(0, 90, 0), Space.Self);
        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint.position, movementSpeed * Time.deltaTime);
    }
    private void SwitchWaypoint()
    {
        // Switch to B if waypoint was A, and vise versa.
        if (targetWaypoint == waypoints[0])
        {
            targetWaypoint = waypoints[1];
            _patrolDirectionRight = true;
        }
        else
        {
            targetWaypoint = waypoints[0];
            _patrolDirectionRight = false;
        }
    }

    private void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, movementSpeed * Time.deltaTime);
    }

    public void ReturnToWaypoint()
    {
        if (transform.position == targetWaypoint.position)
        {
            returnToWaypoint = false;
            EnemyAnimator.StopPlayback();
            EnemyAnimator.Play("Idle");
        }

        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint.position, movementSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ! Deduct Life
        if (collision.transform.CompareTag("Player"))
            Debug.Log("GAME OVER");
    }

    private void FireBullet()
    {
        GameObject _bullet = Instantiate(projectile, transform.position, Quaternion.identity);
    }

}
