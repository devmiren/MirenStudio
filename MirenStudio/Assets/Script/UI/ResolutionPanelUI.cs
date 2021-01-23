using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionPanelUI : MonoBehaviour
{
    [SerializeField]
    public GameObject resolutionPanel;
    
    [SerializeField]
    public Button close, setResolution;

    [SerializeField]
    public InputField width, height;

    [SerializeField]
    public Text errorMessage;

    [SerializeField]
    public Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        width.text = ((int)(Screen.width * mainCamera.rect.width)).ToString();
        height.text = ((int)(Screen.height * mainCamera.rect.height)).ToString();

        setResolution.onClick.AddListener(setResolutionEvent);
        close.onClick.AddListener(closeEvent);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void closeEvent()
    {
        Debug.Log("Called closeEvent");
        resolutionPanel.SetActive(false);
    }

    private void setResolutionEvent()
    {
        Debug.Log("Called setResolutionEvent");
        float width_value, height_value;
        try
        {
            width_value = int.Parse(width.text) / (float)Screen.width;
            height_value = int.Parse(height.text) / (float)Screen.height;
        }
        catch(Exception e)
        {
            errorMessage.text = "width or height is not a number";
            return;
        }

        width_value = Math.Min(Math.Max(width_value, 0.1f), 1);
        height_value = Math.Min(Math.Max(height_value, 0.1f), 1);
        Rect rect = new Rect(1 - width_value, 0, width_value, height_value);
        mainCamera.rect = rect;

        width.text = ((int)(width_value * Screen.width)).ToString();
        height.text = ((int)(height_value * Screen.height)).ToString();
    }
}
