using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelToggleScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(MenuOpenButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [SerializeField]
    private GameObject menuPanel;

    private Button button;

    public void MenuOpenButtonClick()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    public void MenuOpenButtonChangeSprite()
    {
        button.GetComponent<Image>().sprite = Resources.Load<Sprite>(menuPanel.activeSelf ? "Button/MenuOpenImage" : "Button/MenuCloseImage");
    }
}
