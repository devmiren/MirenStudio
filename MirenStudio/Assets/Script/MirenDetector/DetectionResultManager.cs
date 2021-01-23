using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using VRM;
using UniGLTF;
namespace MirenDetector
{
    public class DetectionResultManager : MonoBehaviour
    {
        [SerializeField]
        public bool autoBlinkFlag = true, emotionUpdateFlag = false;

        [SerializeField]
        public float yaw_scale = 1.0f, pitch_scale = 1.0f;

        [SerializeField]
        public float arm_pitch = -75.0f;

        [SerializeField]
        public int update_per_detection = 2;

        [SerializeField]
        public float smooth_factor = 0.2f;

        private int remain_frame = 0;

        private DetectorCommunicator communicator;
        private DetectionResult result;

        private VRMBlendShapeProxy proxy;
        private Animator animator;

        void Start()
        {
            communicator = new DetectorCommunicator();
            result = new DetectionResult();
            proxy = GetComponent<VRMBlendShapeProxy>();
            animator = GetComponent<Animator>();

            PoseInit();
            AutoBlinkStart();
            HeadDirectionInit();
            EyeDirectionInit();
            MouthShapeInit();
        }

        void FixedUpdate()
        {
            DetectionResult result_out;
            if (communicator.result.TryDequeue(out result_out)) result = result_out;

            PoseUpdate();
            if (autoBlinkFlag) AutoBlinkUpdate();
            HeadDirectionUpdate();
            EyeDirectionUpdate();
            MouthShapeUpdate();
            if(emotionUpdateFlag) EmotionUpdate();

            proxy.Apply();

            remain_frame = (remain_frame + 1) % update_per_detection;
        }
        private void OnDestroy()
        {
            communicator.Stop();
        }
        #region pose
        private OffsetOnTransform leftArm, rightArm;
        private void PoseInit()
        {
            leftArm = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
            rightArm = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
        }

        private void PoseUpdate()
        {
            leftArm.Transform.rotation = Quaternion.AngleAxis(-arm_pitch, Vector3.forward);
            rightArm.Transform.rotation = Quaternion.AngleAxis(arm_pitch, Vector3.forward);
        }
        #endregion
        #region auto_blink
        private double nextBlink, lastBlink;
        private const double blinkTime = 0.1;
        public void AutoBlinkStart()
        {
            nextBlink = UnityEngine.Random.Range(3.0f, 5.0f);
            lastBlink = Time.time;
            autoBlinkFlag = true;
        }
        public void AutoBlinkStop()
        {
            autoBlinkFlag = false;
        }

        private void AutoBlinkUpdate()
        {
            double currentTime = Time.time;
            if(currentTime > nextBlink + lastBlink + 0.1)
            {
                nextBlink = UnityEngine.Random.Range(3.0f, 5.0f);
                lastBlink = currentTime;
                VRMBlendShapeProxyExtensions.ImmediatelySetValue(proxy, BlendShapePreset.Blink, 0);
            }
            else if(currentTime > nextBlink + lastBlink)
            {
                VRMBlendShapeProxyExtensions.ImmediatelySetValue(proxy, BlendShapePreset.Blink, 1 - (float)Math.Abs(nextBlink + lastBlink + 0.05 - currentTime) * 20);
            }
        }
        #endregion
        #region face_direction

        private OffsetOnTransform head, neck;
        private Quaternion prevHeadDirection, targetHeadDirection;
        void HeadDirectionInit()
        {
            prevHeadDirection = targetHeadDirection = Quaternion.identity;
            head = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.Head));
            neck = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.Neck));
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
            head.Transform.rotation = faceAngle;
            neck.Transform.rotation = half;
        }
        #endregion
        #region eye_direction

        private Vector2 prevLeftEyeDirection, targetLeftEyeDirection;
        private Vector2 prevRightEyeDirection, targetRightEyeDirection;
        VRMLookAtHead lookAtHead;
        void EyeDirectionInit()
        {
            prevLeftEyeDirection = targetLeftEyeDirection = Vector2.zero;
            prevRightEyeDirection = targetRightEyeDirection = Vector2.zero;
            lookAtHead = GetComponent<VRMLookAtHead>();
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
            float ratio = (remain_frame + 1) / (float)update_per_detection;
            Vector2 leftEyeDirection = prevLeftEyeDirection * (1 - ratio) + targetLeftEyeDirection * ratio;
            Vector2 rightEyeDirection = prevRightEyeDirection * (1 - ratio) + targetRightEyeDirection * ratio;

            float yaw = (leftEyeDirection.x + rightEyeDirection.x) * 0.5f * 180.0f / (float)Math.PI * yaw_scale;
            float pitch = (leftEyeDirection.y + rightEyeDirection.y) * 0.5f * 180.0f / (float)Math.PI * pitch_scale;
            lookAtHead.RaiseYawPitchChanged(yaw, pitch);
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

            BlendShapePreset[] PivotKey = new BlendShapePreset[3] { BlendShapePreset.A, BlendShapePreset.I, BlendShapePreset.O };
            Vector2[] Pivot = new Vector2[4] { new Vector2(1.1f, 0.5f), new Vector2(1.1f, 0.3f), new Vector2(0.8f, 0.6f), new Vector2(1.0f, 0.0f) };
            double[] dist = new double[4];
            double total = 0;
            for (int i = 0; i < 3; i++) dist[i] = Math.Exp((Pivot[i] - mouthDiameter).magnitude * -4.0);
            dist[3] = Math.Exp(Math.Abs(mouthDiameter.y) * -4.0);

            for (int i = 0; i < 4; i++) total += dist[i];
            for (int i = 0; i < 4; i++) dist[i] /= total;

            if (dist[3] > 0.5f) for (int i = 0; i < 3; i++) dist[i] = 0.0f;

            for (int i = 0; i < 3; i++)
            {
                VRMBlendShapeProxyExtensions.ImmediatelySetValue(proxy, PivotKey[i], (float)dist[i]);
            }
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

            VRMBlendShapeProxyExtensions.ImmediatelySetValue(proxy, BlendShapePreset.Angry, emotion[4]);
            VRMBlendShapeProxyExtensions.ImmediatelySetValue(proxy, BlendShapePreset.Fun, emotion[1]);
            VRMBlendShapeProxyExtensions.ImmediatelySetValue(proxy, BlendShapePreset.Sorrow, emotion[3]);
        }
        #endregion
    }
}