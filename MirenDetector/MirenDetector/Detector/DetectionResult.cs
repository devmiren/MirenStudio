using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using OpenCvSharp;

namespace MirenDetector
{
    public class FacialLandmark
    {
        public Point2f[] location = new Point2f[68];
        public Point2f this[int idx]
        {
            get => location[idx];
            set => location[idx] = value;
        }
    }

    // {'neutral':0, 'happiness':1, 'surprise':2, 'sadness':3, 'anger':4, 'disgust':5, 'fear':6, 'contempt':7}
    public class EmotionVector
    {
        public float[] emotion = new float[8];
        public float this[int idx]
        {
            get => emotion[idx];
            set => emotion[idx] = value;
        }
    }

    public class DetectionResult
    {
        public Rect? face;
        public FacialLandmark faceLandmark;
        public Quaternion faceAngle;
        public Point2f mouthDiameter;
        public Point2f leftEyeDirection, leftEyeIris;
        public Point2f rightEyeDirection, rightEyeIris;
        public EmotionVector emotion;

        private void WriteToByteArray(byte[] arr, ref int top, params float[] value)
        {
            for(int i = 0; i < value.Length; i++)
            {
                Array.Copy(System.BitConverter.GetBytes(value[i]), 0, arr, top, sizeof(float)); top += 4;
            }
        }
        private void WriteToByteArray(byte[] arr, ref int top, params int[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                Array.Copy(System.BitConverter.GetBytes(value[i]), 0, arr, top, sizeof(int)); top += 4;
            }
        }
        private void WriteToByteArray(byte[] arr, ref int top, params byte[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                Array.Copy(System.BitConverter.GetBytes(value[i]), 0, arr, top, sizeof(byte)); top += 1;
            }
        }

        public byte[] ToByteArray()
        {
            if (!face.HasValue) throw new ArgumentException("Result is null");
            int size = sizeof(float) * (4 + 2 + 2 + 2 + 8), top = 0;
            byte[] arr = new byte[size];
            WriteToByteArray(arr, ref top, faceAngle.X, faceAngle.Y, faceAngle.Z, faceAngle.W);
            WriteToByteArray(arr, ref top, mouthDiameter.X, mouthDiameter.Y);
            WriteToByteArray(arr, ref top, leftEyeDirection.X, leftEyeDirection.Y);
            WriteToByteArray(arr, ref top, rightEyeDirection.X, rightEyeDirection.Y);
            //WriteToByteArray(arr, ref top, emotion.emotion);
            return arr;
        }
    }
}
