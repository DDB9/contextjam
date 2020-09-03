using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Initialize the singleton instance
    public static GameManager Instance { get; private set; }

    // Initialize the private variables
    private PlayerController player = null;
    private Cursor _cursor = null;
    private UIManager uiManager = null;
    private CameraController _camera = null;

    public PlayerController Player
    {
        get { return player; }
    }

    public Cursor _Cursor
    {
        get { return _cursor; }
    }

    public UIManager UiManager
    {
        get { return uiManager; }
    }

    public CameraController _Camera
    {
        get { return _camera; }
    }

    // Awake is called before anything else
    private void Awake()
    {
        // Make sure there is ever only one singleton instance active at a time
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize the game manager before any other scripts can access it
        Initialize();
    }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    // Initialize the game manager
    private void Initialize()
    {
        player = FindObjectOfType<PlayerController>();
        _cursor = FindObjectOfType<Cursor>();
        uiManager = FindObjectOfType<UIManager>();
        _camera = FindObjectOfType<CameraController>();
    }
}
