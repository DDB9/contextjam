using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSystem : MonoBehaviour
{
    //laser parameters
    public GameObject laser;
    public GameObject beamHit;
    public GameObject laserSystem;
    public GameObject laser2;
    public GameObject beamHit2;
    public GameObject laserSystem2;
    public float length = 15.0f;

    // Start is called before the first frame update
    void Start()
    {
        beamHit.SetActive(false);
        beamHit2.SetActive(false);
        laserSystem2.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Laserbeam();
    }

    private void Laserbeam()
    {
        Vector3 direction = transform.up;
        RaycastHit hit = new RaycastHit();
        LayerMask mask = LayerMask.GetMask("Solid", "Shield");
        bool hitSurface = (Physics.Raycast(transform.position, direction, out hit, length, mask));

        LineRenderer LaserLineRenderer = laser.GetComponent<LineRenderer>();
        LaserLineRenderer.SetPosition(0, transform.position);

        if (hitSurface)
        {
            if (hit.transform.tag == "Player")
            {
                laserSystem2.SetActive(true);

                Vector3 bounceDirection = Vector3.Reflect(direction, hit.normal);

                RaycastHit hit2 = new RaycastHit();
                LayerMask mask2 = LayerMask.GetMask("Solid");
                bool hitSurface2 = (Physics.Raycast(beamHit.transform.position, bounceDirection, out hit2, length, mask2));

                LineRenderer LaserLineRenderer2 = laser2.GetComponent<LineRenderer>();
                LaserLineRenderer2.SetPosition(0, beamHit.transform.position);
                if (hitSurface2)
                {
                    beamHit2.SetActive(true);
                    beamHit2.transform.position = hit2.point;
                    LaserLineRenderer2.SetPosition(1, hit2.point);
                }
                else
                {
                    beamHit2.SetActive(false);
                    LaserLineRenderer2.SetPosition(1, bounceDirection * length);
                }
            }
            else
            {
                laserSystem2.SetActive(false);
                beamHit2.SetActive(false);
            }

            beamHit.SetActive(true);
            beamHit.transform.position = hit.point;
            LaserLineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            beamHit.SetActive(false);
            laserSystem2.SetActive(false);
            beamHit2.SetActive(false);
            LaserLineRenderer.SetPosition(1, transform.up * length);
        }
    }
}
