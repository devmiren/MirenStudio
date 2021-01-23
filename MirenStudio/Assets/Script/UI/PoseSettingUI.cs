using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MirenDetector;

public class PoseSettingUI : MonoBehaviour
{
    [SerializeField]
    public GameObject vrmModel;

    public void SetArmPitch(float angle)
    {
        DetectionResultManager detector = vrmModel.GetComponent<DetectionResultManager>();
        if (detector != null) detector.arm_pitch = angle;
    }
}
