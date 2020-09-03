using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Initialize the private variables
    private Slider jumpChargeSlider = null;

    public Slider JumpChargeSlider
    {
        get { return jumpChargeSlider; }
    }

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateSliders();
    }

    // Initialize the UI manager
    private void Initialize()
    {
        jumpChargeSlider = transform.Find("JumpCharge").GetComponent<Slider>();
    }

    // Update the sliders
    private void UpdateSliders()
    {
        jumpChargeSlider.gameObject.SetActive(GameManager.Instance.Player.JumpIsCharging);
    }
}
