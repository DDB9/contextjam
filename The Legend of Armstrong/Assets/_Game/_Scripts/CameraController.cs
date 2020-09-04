using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Initialize the private variables
    private Camera cameraComp = null;

    public Camera CameraComp
    {
        get { return cameraComp; }
    }

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    // Initialize the camera controller
    private void Initialize()
    {
        cameraComp = GetComponent<Camera>();
    }
}
