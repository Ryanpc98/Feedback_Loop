using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Image fillArea;
    [SerializeField] private TextMeshProUGUI text;

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        if (currentValue <= 0)
        {
            //fillArea.color = Color.clear;
        }
        else
        {
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