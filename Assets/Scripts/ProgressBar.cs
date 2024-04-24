using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField]
    private float speedMultiplier = 1;

    private Slider slider;
    private float targetValue;
 
    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Update()
    {
        UpdateValue();
    }

    public void SetMaxValue(float maxValue)
    {
        if(slider == null)
            slider = GetComponent<Slider>();
        slider.maxValue = maxValue;
    }

    public void SetValue(float value)
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if(slider != null)
            targetValue = value;
    }

    public void SetSpeedMultiplier(float speedMultiplier)
    {
        this.speedMultiplier = speedMultiplier;
    }

    private void UpdateValue()
    {
        if(slider.value != targetValue)
        {
            slider.value = Mathf.MoveTowards(slider.value, targetValue, 125f * speedMultiplier * Time.deltaTime);
        }
    }
}
