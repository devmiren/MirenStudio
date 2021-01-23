using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSettingUI : MonoBehaviour
{
    [SerializeField]
    public Camera camera;

    private float distance = 0.7f;
    private float height = 1.65f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private void CameraUpdate()
    {
        camera.transform.rotation = Quaternion.Euler(pitch, yaw + 180, 0);
        camera.transform.position = new Vector3(0, height, 0) + camera.transform.rotation * new Vector3(0, 0, -distance);
    }

    public void SetCameraDistance(float value)
    {
        distance = value;
        CameraUpdate();
    }

    public void SetCameraHeight(float value)
    {
        height = value;
        CameraUpdate();
    }

    public void SetCameraYaw(float value)
    {
        yaw = value;
        CameraUpdate();
    }

    public void SetCameraPitch(float value)
    {
        pitch = value;
        CameraUpdate();
    }
}
