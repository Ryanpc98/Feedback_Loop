using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingEnergyBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Image fillArea;
    [SerializeField] private TextMeshProUGUI text;

    public void UpdateEnergyBar(float currentValue, float maxValue)
    {
        if (currentValue <= 0)
        {
            fillArea.color = Color.clear;
        }
        else
        {
            if (fillArea.color == Color.clear)
            {
                fillArea.color = new Color(0f, 0.65f, 0.83f);
            }
            slider.value = currentValue / maxValue;
        }
        text.text = currentValue.ToString() + "/" + maxValue.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
        transform.position = target.position + offset;
    }
}