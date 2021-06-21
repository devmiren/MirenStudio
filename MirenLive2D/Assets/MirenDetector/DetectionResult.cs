using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirenDetector
{
    // {'neutral':0, 'happiness':1, 'surprise':2, 'sadness':3, 'anger':4, 'disgust':5, 'fear':6, 'contempt':7}
    public class EmotionVector
    {
        public float[] emotion;
        public float this[int idx]
        {
            get => emotion[idx];
            set => emotion[idx] = value;
        }

        public EmotionVector()
        {
            emotion = new float[8];
        }

        public EmotionVector(float[] _emotion)
        {
            emotion = (float[]) _emotion.Clone();
        }

        public EmotionVector Clone()
        {
            return new EmotionVector(emotion);
        }
    }

    public class DetectionResult
    {
        public Quaternion faceAngle;
        public Vector2 mouthDiameter;
        public Vector2 leftEyeDirection;
        public Vector2 rightEyeDirection;
        public EmotionVector emotion;

        private static float FloatFromByteArray(byte[] arr, ref int top)
        {
            float value = System.BitConverter.ToSingle(arr, top); top += 4;
            return value;
        }

        private static int IntFromByteArray(byte[] arr, ref int top)
        {
            int value = System.BitConverter.ToInt32(arr, top); top += 4;
            return value;
        }

        public DetectionResult()
        {
            faceAngle = Quaternion.identity;
            mouthDiameter = new Vector2(1, 0);
            leftEyeDirection = rightEyeDirection = new Vector2(0, 0);
            emotion = new EmotionVector();
        }

        public DetectionResult(Quaternion _faceAngle, Vector2 _mouthDiameter, EmotionVector _emotion, Vector2 _leftEyeDirection, Vector2 _rightEyeDirection)
        {
            faceAngle = _faceAngle;
            mouthDiameter = _mouthDiameter;
            leftEyeDirection = _leftEyeDirection;
            rightEyeDirection = _rightEyeDirection;
            emotion = _emotion.Clone();
        }

        public DetectionResult(byte[] arr)
        {
            emotion = new EmotionVector();

            int top = 0;
            faceAngle.x = FloatFromByteArray(arr, ref top);
            faceAngle.y = FloatFromByteArray(arr, ref top);
            faceAngle.z = FloatFromByteArray(arr, ref top);
            faceAngle.w = FloatFromByteArray(arr, ref top);
            mouthDiameter.x = FloatFromByteArray(arr, ref top);
            mouthDiameter.y = FloatFromByteArray(arr, ref top);
            leftEyeDirection.x = FloatFromByteArray(arr, ref top);
            leftEyeDirection.y = FloatFromByteArray(arr, ref top);
            rightEyeDirection.x = FloatFromByteArray(arr, ref top);
            rightEyeDirection.y = FloatFromByteArray(arr, ref top);
            for (int i = 0; i < 8; i++) emotion[i] = FloatFromByteArray(arr, ref top);

            faceAngle = ReverseZ(faceAngle);
            faceAngle = new Quaternion(1, 0, 0, 0) * ReverseZ(faceAngle) * new Quaternion(1, 0, 0, 0);
        }

        public DetectionResult Clone()
        {
            return new DetectionResult(faceAngle, mouthDiameter, emotion, leftEyeDirection, rightEyeDirection);
        }
        private static Vector3 ReverseZ(Vector3 v)
        {
            return new Vector3(v.x, v.y, -v.z);
        }
        private static Quaternion ReverseZ(Quaternion q)
        {
            float angle;
            Vector3 axis;
            q.ToAngleAxis(out angle, out axis);
            return Quaternion.AngleAxis(angle, ReverseZ(axis));
        }
    }
}
