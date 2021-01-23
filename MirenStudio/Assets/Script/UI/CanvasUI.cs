using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

public class CanvasUI : MonoBehaviour
{
    [SerializeField]
    public Text text, debugText;

    [SerializeField]
    public Slider mouthWidthSlider, mouthHeightSlider;

    void Start()
    {
    }

    void Update()
    {
    }


    private int counter = 0;
    public void TestClick()
    {
        counter += 1;
        text.text = counter.ToString();
    }
}
