using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TransparentWindow : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
    [DllImport("user32.dll")]
    private static extern uint SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);
    [DllImport("user32.dll")]
    private static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);

    const int GWL_EXSTYLE = -20;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    // Start is called before the first frame update

    IntPtr hWnd;

    void Start()
    {
#if !UNITY_EDITOR
        hWnd = GetActiveWindow();

        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if !UNITY_EDITOR
        SetClickthrough(Physics2D.OverlapPoint(Input.mousePosition));
#endif
    }

    private void SetClickthrough(bool clickthrough)
    {
        if (clickthrough) 
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        else
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
    }
}
