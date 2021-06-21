using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.Motion;

public class MyMotionPlayer : MonoBehaviour
{
    CubismMotionController _motionController;

    [SerializeField]
    AnimationClip animation;

    void Start()
    {
        _motionController = GetComponent<CubismMotionController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _motionController.PlayAnimation(animation, isLoop: false);
        }
        else if (!_motionController.IsPlayingAnimation())
        {
            _motionController.StopAllAnimation();
        }
    }
}
