using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using OpenCvSharp;

namespace MirenDetector
{
    class HeadPoseDetector
    {
        private double[] rvec_last = { 0, 0, 0 }, tvec_last = { 0.0, 0.0, 3000.0 };

        int[] MouthIdx = new int[4] { 48, 54, 62, 66 };

        private Vector2 LandmarkToVector2(FacialLandmark landmark, int idx)
        {
            return new Vector2(landmark[idx].X, landmark[idx].Y);
        }

        public void landmarkToPose(CameraMatrix camera, Face3DPoint face3DPoints, FacialLandmark landmark, bool smoothing, out Quaternion angle, out Point2f mouth)
        {
            int[] keypoint = Face3DPoint.keypointIdx;
            Point2f[] key2DPoints = new Point2f[keypoint.Length];
            Point3f[] key3DPoints = new Point3f[keypoint.Length];
            for (int i = 0; i < keypoint.Length; i++) key2DPoints[i] = landmark[keypoint[i]];
            for (int i = 0; i < keypoint.Length; i++) key3DPoints[i] = face3DPoints[i];

            double[] rvec_arr = (double[])rvec_last.Clone(), tvec_arr = (double[])tvec_last.Clone();
            Cv2.SolvePnP(key3DPoints, key2DPoints, camera.arr, null, ref rvec_arr, ref tvec_arr);

            if (tvec_arr[2] > 0) rvec_arr[2] *= -1;
            Vector3 rvec = new Vector3((float)rvec_arr[0], (float)rvec_arr[1], (float)rvec_arr[2]);
            float a = rvec.Length();
            Quaternion q = Quaternion.CreateFromAxisAngle(rvec / a, a);

            Vector2[] mouthPoint = new Vector2[4];
            Vector2[] eyeBorder = new Vector2[2];
            for (int i = 0; i < 4; i++) mouthPoint[i] = LandmarkToVector2(landmark, MouthIdx[i]);
            eyeBorder[0] = LandmarkToVector2(landmark, 36);
            eyeBorder[1] = LandmarkToVector2(landmark, 47);
            Quaternion X = q * (new Quaternion(1.0f, 0.0f, 0.0f, 0.0f)) / q, Y = q * (new Quaternion(0.0f, 1.0f, 0.0f, 0.0f)) / q;
            double dx = (mouthPoint[0] - mouthPoint[1]).Length() / Math.Max(0.5f, Math.Sqrt(sq(X.X) + sq(X.Y)));
            double dy = (mouthPoint[2] - mouthPoint[3]).Length() / Math.Max(0.5f, Math.Sqrt(sq(Y.X) + sq(Y.Y)));
            double unit = (eyeBorder[0] - eyeBorder[1]).Length() / Math.Max(0.5f, Math.Sqrt(sq(X.X) + sq(X.Y))) * (47.5f / 80.0f * 1.2f);

            angle = q;
            mouth = new Point2f((float)(dx / unit), (float)(dy / dx));
        }

        private double sq(double x) { return x * x; }
        private float sq(float x) { return x * x; }
    }
}
