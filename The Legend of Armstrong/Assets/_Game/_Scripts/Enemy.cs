using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType {STATIC, PATROLLING, RANGED};
[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    public EnemyType enemyType;
    public Animator EnemyAnimator;
    public GameObject projectile;
    public float fireDelay = 1f;
    public float movementSpeed;
    public bool alerted;
    public bool disablePatrol;
    public bool ranged;

    [HideInInspector]
    public Transform targetWaypoint;
    [HideInInspector]
    public bool returnToWaypoint;
    [HideInInspector]
    public bool invokeRepeatingOnce;
    
    private GameObject _player;
    private List<Transform> _waypoints = new List<Transform>();
    private int _waypointIndex = 0;
    private bool _playAnimationOnce;

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");

        foreach (Transform child in transform.parent.GetChild(1).transform)
            _waypoints.Add(child);

        targetWaypoint = _waypoints[_waypointIndex];
    }

    private void Update()
    {
        switch (enemyType)
        {
            case EnemyType.STATIC:
                if (alerted) ChasePlayer();
                else if (returnToWaypoint) ReturnToWaypoint();
                break;

            case EnemyType.PATROLLING:
                if (!_playAnimationOnce)
                {
                    EnemyAnimator.Play("EnemyMove");
                    _playAnimationOnce = true;
                }
                Patrol();
                break;

            case EnemyType.RANGED:
                if (alerted)
                {
                    fireDelay -= Time.deltaTime;
                    if (fireDelay <= 0)
                    {
                        FireBullet();
                        fireDelay = 1f;
                    }
                }
                else CancelInvoke();
                break;
        }
    }

    private void Patrol()
    {
        // Switch waypoints if enemy has reached a waypoint
        if (transform.position == targetWaypoint.position) SwitchWaypoint();
        else if (transform.position == targetWaypoint.position) SwitchWaypoint();

        // Move enemy to target waypoint.
        transform.LookAt(targetWaypoint.position);
        transform.Rotate(new Vector3(0, 90, 0), Space.Self);
        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint.position, movementSpeed * Time.deltaTime);
    }
    private void SwitchWaypoint()
    {
        _waypointIndex++;
        if (_waypointIndex >= _waypoints.Count) _waypointIndex = 0;

        targetWaypoint = _waypoints[_waypointIndex];
    }

    private void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, _player.transform.position, movementSpeed * Time.deltaTime);
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ! Deduct Life
        if (collision.transform.CompareTag("Player"))
            Debug.Log("GAME OVER");
    }

    private void FireBullet()
    {
        EnemyAnimator.Play("EnemyShoot");
        GameObject _bullet = Instantiate(projectile, transform.position, Quaternion.identity);
    }
}
