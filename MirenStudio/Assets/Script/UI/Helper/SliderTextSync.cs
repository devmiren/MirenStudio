using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderTextSync : MonoBehaviour
{
    [SerializeField]
    public Slider slider;

    [SerializeField]
    public InputField inputField;

    public void Start()
    {
        inputField.text = slider.value.ToString();
    }

    public void SliderValueUpdate(float value)
    {
        inputField.text = value.ToString();
    }

    public void TextValueUpdate(string value)
    {
        try
        {
            slider.value = Single.Parse(value);
        }
        catch(Exception e)
        {

        }
    }
}
