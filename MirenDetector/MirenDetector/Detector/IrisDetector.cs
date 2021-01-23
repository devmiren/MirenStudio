using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Numerics;
using OpenCvSharp;

namespace MirenDetector
{
    class IrisDetector
    {
        static readonly double SIN_LEFT_THETA = Math.Sin(Math.PI / 4);
        static readonly double COS_LEFT_THETA = Math.Cos(Math.PI / 4);
        static readonly double SIN_UP_THETA = Math.Sin(Math.PI / 24);
        static readonly double COS_UP_THETA = Math.Cos(Math.PI / 24);

        static readonly Vector3 left_3d = new Vector3((float)(SIN_LEFT_THETA * COS_UP_THETA), (float)-SIN_UP_THETA, (float)(COS_LEFT_THETA * COS_UP_THETA));
        static readonly Vector3 right_3d = new Vector3((float)(-SIN_LEFT_THETA * COS_UP_THETA), (float)-SIN_UP_THETA, (float)(COS_LEFT_THETA * COS_UP_THETA));

        InferenceSession session;

        public IrisDetector()
        {
            SessionOptions so = new SessionOptions();
            so.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED;
            so.AppendExecutionProvider_CUDA(0);
            session = new InferenceSession("Assets/IrisLandmark.onnx", so);
        }


        public Point2f IrisDetect(Mat image)
        {
            int image_width = image.Width, image_height = image.Height;
            Mat normalized = new Mat();
            MatrixHelper.Resize(image, 64, 64).ConvertTo(normalized, MatType.CV_32FC3);
            Cv2.Normalize(normalized, normalized, alpha: -1, beta: 1, normType: NormTypes.MinMax);

            Tensor<float> input = MatrixHelper.MatToTensor(ImageFormat.BGR, 0, 1, normalized);
            var inputs = new List<NamedOnnxValue>() { NamedOnnxValue.CreateFromTensor<float>("input_1", input) };

            using (var results = session.Run(inputs))
            {
                float[] iris = null, contour = null;
                foreach (var r in results)
                {
                    float[] value = r.AsTensor<float>().ToDenseTensor().Buffer.ToArray();
                    if (r.Name == "output_eyes_contours_and_brows") contour = value;
                    if (r.Name == "output_iris") iris = value;
                }

                return new Point2f(iris[0] / 64 * image_width, iris[1] / 64 * image_height);
            }
        }

        public Vec2f IrisToDirection(Point2f left, Point2f right, Point2f pupil, Quaternion q)
        {
            Quaternion q_inv = Quaternion.Inverse(q);
            Point2f Xaxis = (right - left);
            Point2f Yaxis = new Point2f(-Xaxis.Y, Xaxis.X);
            double dX = Xaxis.DotProduct(pupil - left) / Xaxis.DotProduct(Xaxis);
            double dY = Yaxis.DotProduct(pupil - left) / Yaxis.DotProduct(Yaxis);

            Quaternion left_rotated = q * new Quaternion(left_3d.X, left_3d.Y, left_3d.Z, 0.0f) / q;
            Quaternion right_rotated = q * new Quaternion(right_3d.X, right_3d.Y, right_3d.Z, 0.0f) / q;

            Point2f left_projected = new Point2f(left_rotated.X, left_rotated.Y);
            Point2f right_projected = new Point2f(right_rotated.X, right_rotated.Y);

            Point2f Xaxis_projected = right_projected - left_projected;
            Point2f Yaxis_projected = new Point2f(-Xaxis_projected.Y, Xaxis_projected.X);

            Point2f pupil_projected = left_projected + Xaxis_projected * dX + Yaxis_projected * dY;
            double norm2 = pupil_projected.DotProduct(pupil_projected);
            if (norm2 >= 1.0)
            {
                pupil_projected *= 1.0 / Math.Sqrt(norm2);
                norm2 = 1.0;
            }
            Point3f pupil_3d = new Point3f(pupil_projected.X, pupil_projected.Y, (float)Math.Sqrt(1 - norm2));
            Quaternion pupil_rotated = q_inv * new Quaternion(pupil_3d.X, pupil_3d.Y, pupil_3d.Z, 0.0f) / q_inv;

            double X = pupil_rotated.X, Y = pupil_rotated.Y;
            double w = Math.Sqrt(Math.Max(1e-18f, 1 - Y * Y)), h = Math.Sqrt(Math.Max(1e-18f, 1 - X * X));
            double theta_inv = Math.Max(-1, Math.Min(1, X / w)), phi_inv = Math.Max(-1, Math.Min(1, Y / h));
            return new Vec2f(-(float)Math.Asin(theta_inv), (float)Math.Asin(phi_inv));
        }
    }
}
