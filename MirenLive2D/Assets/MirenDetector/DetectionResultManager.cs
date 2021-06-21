//*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.LookAt;

namespace MirenDetector
{
    public class DetectionResultManager : MonoBehaviour, ICubismLookTarget, ICubismUpdatable
    {
        [SerializeField]
        public bool autoBlinkFlag = true, emotionUpdateFlag = false;

        [SerializeField]
        public float yaw_scale = 1.0f, pitch_scale = 1.0f;

//        [SerializeField]
//        public float arm_pitch = -75.0f;

        [SerializeField]
        public int update_per_detection = 2;

        [SerializeField]
        public float smooth_factor = 0.2f;

        private int remain_frame = 0;

        private DetectorCommunicator communicator;
        private DetectionResult result;

        void Start()
        {
            communicator = new DetectorCommunicator();
            result = new DetectionResult();

            PoseInit();
            AutoBlinkStart();
            HeadDirectionInit();
            EyeDirectionInit();
            MouthShapeInit();

            HasUpdateController = (GetComponent<CubismUpdateController>() != null);
        }

        void FixedUpdate()
        {
            DetectionResult result_out;
            if (communicator.result.TryDequeue(out result_out)) result = result_out;

            remain_frame = (remain_frame + 1) % update_per_detection;
        }
        private void OnDestroy()
        {
            communicator.Stop();
        }
        #region pose
        private void PoseInit()
        {
        }

        private void PoseUpdate()
        {
        }
        #endregion
        #region auto_blink
        private double nextBlink, lastBlink;
        private const double blinkTime = 0.1;
        CubismEyeBlinkController cubismEyeBlinkController;
        public void AutoBlinkStart()
        {
            nextBlink = UnityEngine.Random.Range(3.0f, 5.0f);
            lastBlink = Time.time;
            autoBlinkFlag = true;
            cubismEyeBlinkController = GetComponent<CubismEyeBlinkController>();
        }
        public void AutoBlinkStop()
        {
            autoBlinkFlag = false;
        }

        private void AutoBlinkUpdate()
        {
            double currentTime = Time.time;
            if (currentTime > nextBlink + lastBlink + 0.1)
            {
                nextBlink = UnityEngine.Random.Range(3.0f, 5.0f);
                lastBlink = currentTime;
                cubismEyeBlinkController.EyeOpening = 1.0f;
            }
            else if (currentTime > nextBlink + lastBlink)
            {
                cubismEyeBlinkController.EyeOpening = (float)Math.Abs(nextBlink + lastBlink + 0.05 - currentTime) * 20;
            }
        }
        #endregion
        #region face_direction

        private Quaternion prevHeadDirection, targetHeadDirection;
        private CubismParameter faceEulerX, faceEulerY, faceEulerZ;
        void HeadDirectionInit()
        {
            prevHeadDirection = targetHeadDirection = Quaternion.identity;

            var model = this.FindCubismModel();
            CubismParameter[] arr = model.Parameters;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Id == "ParamAngleX") faceEulerX = arr[i];
                if (arr[i].Id == "ParamAngleY") faceEulerY = arr[i];
                if (arr[i].Id == "ParamAngleZ") faceEulerZ = arr[i];
            }
        }

        void HeadDirectionUpdate()
        {
            if (remain_frame == 0)
            {
                prevHeadDirection = targetHeadDirection;
                targetHeadDirection = Quaternion.Slerp(targetHeadDirection, result.faceAngle, smooth_factor);
            }
            Quaternion faceAngle = Quaternion.Slerp(prevHeadDirection, targetHeadDirection, (float)(remain_frame + 1) / update_per_detection);
            Quaternion half = Quaternion.Slerp(Quaternion.identity, faceAngle, 0.5f);

            Vector3 param = faceAngle.eulerAngles;
            if (param.x > 180) param.x -= 360;
            if (param.y > 180) param.y -= 360;
            if (param.z > 180) param.z -= 360;

            Debug.Log(param);
            faceEulerX.BlendToValue(CubismParameterBlendMode.Override, param.y);
            faceEulerY.BlendToValue(CubismParameterBlendMode.Override, -param.x);
            faceEulerZ.BlendToValue(CubismParameterBlendMode.Override, param.z);
        }
        #endregion
        #region eye_direction

        private Vector2 prevLeftEyeDirection, targetLeftEyeDirection;
        private Vector2 prevRightEyeDirection, targetRightEyeDirection;
        void EyeDirectionInit()
        {
            prevLeftEyeDirection = targetLeftEyeDirection = Vector2.zero;
            prevRightEyeDirection = targetRightEyeDirection = Vector2.zero;
        }

        void EyeDirectionUpdate()
        {
            if (remain_frame == 0)
            {
                prevLeftEyeDirection = targetLeftEyeDirection;
                prevRightEyeDirection = targetRightEyeDirection;

                targetLeftEyeDirection = targetLeftEyeDirection * (1 - smooth_factor) + result.leftEyeDirection * smooth_factor;
                targetRightEyeDirection = targetRightEyeDirection * (1 - smooth_factor) + result.rightEyeDirection * smooth_factor;
            }
        }

        public Vector3 GetPosition()
        {
            float ratio = (remain_frame + 1) / (float)update_per_detection;
            Vector2 leftEyeDirection = prevLeftEyeDirection * (1 - ratio) + targetLeftEyeDirection * ratio;
            Vector2 rightEyeDirection = prevRightEyeDirection * (1 - ratio) + targetRightEyeDirection * ratio;
            Vector2 direction = (leftEyeDirection + rightEyeDirection) * 0.5f;
            return new Vector3(direction.x * 2, direction.y * 2, 0.0f);
        }

        public bool IsActive()
        {
            return true;
        }
        #endregion
        #region mouth_shape

        private Vector2 prevMouthDiameter = new Vector2(1.0f, 0.0f);
        private Vector2 targetMouthDiameter = new Vector2(1.0f, 0.0f);
        void MouthShapeInit()
        {
        }

        void MouthShapeUpdate()
        {
            if (remain_frame == 0)
            {
                prevMouthDiameter = targetMouthDiameter;
                targetMouthDiameter = result.mouthDiameter * (1 - smooth_factor) + prevMouthDiameter * smooth_factor;
            }
            float ratio = (remain_frame + 1) / (float)update_per_detection;
            Vector2 mouthDiameter = prevMouthDiameter * (1 - ratio) + targetMouthDiameter * ratio;

            // TODO
        }
        #endregion
        #region emotion
        EmotionVector emotion = new EmotionVector();
        public void EmotionUpdateStart()
        {
            emotionUpdateFlag = true;
        }
        public void EmotionUpdateStop()
        {
            emotionUpdateFlag = false;
        }

        private void EmotionUpdate()
        {
            emotion = result.emotion.Clone();
            // TODO
        }
        #endregion

        #region Live2D
        /// <summary>
        /// Called by cubism update controller. Order to invoke OnLateUpdate.
        /// </summary>
        public int ExecutionOrder
        {
            get { return CubismUpdateExecutionOrder.CubismLookController; }
        }
        /// <summary>
        /// Called by cubism update controller. Needs to invoke OnLateUpdate on Editing.
        /// </summary>
        public bool NeedsUpdateOnEditing
        {
            get { return false; }
        }

        /// <summary>
        /// Model has update controller component.
        /// </summary>
        [HideInInspector]
        public bool HasUpdateController { get; set; }

        /// <summary>
        /// Called by cubism update controller. Updates controller.
        /// </summary>
        public void OnLateUpdate()
        {
            PoseUpdate();
            if (autoBlinkFlag) AutoBlinkUpdate();
            HeadDirectionUpdate();
            EyeDirectionUpdate();
            MouthShapeUpdate();
            if (emotionUpdateFlag) EmotionUpdate();
        }

        /// <summary>
        /// Called by Unity. Updates controller.
        /// </summary>
        private void LateUpdate()
        {
            if (!HasUpdateController)
            {
                OnLateUpdate();
            }
        }
        #endregion
    }
}