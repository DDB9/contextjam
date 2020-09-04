using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Initialize the private variables
    private Slider jumpChargeSlider = null;
    private Text bombText = null;
    public List<Image> halfHearts = new List<Image>();

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
        UpdateUI();
    }

    // Initialize the UI manager
    private void Initialize()
    {
        jumpChargeSlider = transform.Find("JumpCharge").GetComponent<Slider>();
        bombText = transform.Find("Bombs").GetComponentInChildren<Text>();

        foreach (Transform child in transform.Find("HP"))
        {
            if (child.name == "Heart")
            {
                foreach (Transform _child in child)
                {
                    Image halfHeart = _child.GetComponent<Image>();
                    if (halfHeart != null)
                    {
                        halfHearts.Add(halfHeart);
                    }
                }
            }
        }
    }

    // Update the sliders
    private void UpdateUI()
    {
        jumpChargeSlider.gameObject.SetActive(GameManager.Instance.Player.JumpIsCharging);
        jumpChargeSlider.value = 1f - (GameManager.Instance.Player.JumpChargeTimer / GameManager.Instance.Player.jumpChargeDuration);

        //hpText.text = "HP: " + GameManager.Instance.Player.Hp.ToString() + " / " + GameManager.Instance.Player.maxHp.ToString();
        bombText.text = "Bombs: " + GameManager.Instance.Player.Bombs.ToString();

        for (int i = 0; i < halfHearts.Count; i++)
        {
            halfHearts[i].enabled = (i <= (GameManager.Instance.Player.Hp - 1));
        }
    }
}
