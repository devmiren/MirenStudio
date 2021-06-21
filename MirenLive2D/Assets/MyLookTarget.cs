using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.LookAt;

public class MyLookTarget : MonoBehaviour, ICubismLookTarget
{
    public Vector3 GetPosition()
    {
        return Camera.main.ScreenToViewportPoint(Input.mousePosition) - new Vector3(.5f, .5f, 0);
    }

    public bool IsActive()
    {
        return true;
    }
}
