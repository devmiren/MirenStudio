using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using OpenCvSharp;

namespace MirenDetector
{
    class MirenDetectior
    {
        #region flag
        private bool FaceDetectionFlag = false;
        private bool ImageStoreFlag = false;
        private bool SetFacingDirectionFlag = false;

        public void ImageStoreEnable(bool flag) => ManageFlagOption(flag, ref ImageStoreFlag);
        public void FaceDetectionEnable(bool flag) => ManageFlagOption(flag, ref FaceDetectionFlag);
        public void SetFacingDirection(bool flag) => ManageFlagOption(flag, ref SetFacingDirectionFlag);
        private void ManageFlagOption(bool flag, ref bool prev_flag)
        {
            if (flag && !prev_flag) prev_flag = true;
            else if (!flag && prev_flag) prev_flag = false;
        }
        #endregion

        private RetinaFaceDetector retinaFaceDetector;
        private FacialLandmarkDetector pfldLandmarkDetector;
        private HeadPoseDetector headPoseDetector;
        private IrisDetector irisDetector;

        public CameraMatrix cameraMatrix { get; private set; }
        public Face3DPoint face3DPoint { get; private set; }

        public MirenDetectior(int width, int height)
        {
            retinaFaceDetector = new RetinaFaceDetector();
            pfldLandmarkDetector = new FacialLandmarkDetector();
            headPoseDetector = new HeadPoseDetector();
            irisDetector = new IrisDetector();

            face3DPoint = new Face3DPoint();
            cameraMatrix = CameraMatrix.DefaultCamera(width, height);
        }

        public async Task<DetectionResult> DetectAsync(Mat image)
        {
            DetectionResult result = new DetectionResult();
            OpenCvSharp.Rect? rect = retinaFaceDetector.FaceDetect(image);
            if (!rect.HasValue) return null;
            result.face = rect;

            Rect extended = BoxExtension(rect.Value, image.Width, image.Height);
            Mat faceImage = image.SubMat(extended);
            result.faceLandmark = pfldLandmarkDetector.LandmarkDetect(faceImage);
            for(int i = 0; i < 68; i++) 
            {
                result.faceLandmark.location[i].X += extended.X;
                result.faceLandmark.location[i].Y += extended.Y;
            }

            Quaternion faceAngle; Point2f mouthDiameter;
            headPoseDetector.landmarkToPose(cameraMatrix, face3DPoint, result.faceLandmark, true, out faceAngle, out mouthDiameter);
            result.faceAngle = faceAngle; result.mouthDiameter = mouthDiameter;

            Task leftEyeTask = Task.Run(() =>
            {
                Rect leftEye = GetEyeBox(image, result.faceLandmark[36], result.faceLandmark[39]);
                Point2f leftIris = irisDetector.IrisDetect(image.SubMat(leftEye));
                leftIris += leftEye.TopLeft;
                result.leftEyeDirection = irisDetector.IrisToDirection(result.faceLandmark[36], result.faceLandmark[39], leftIris, faceAngle);
                result.leftEyeIris = leftIris;
            });
            Task rightEyeTask = Task.Run(() =>
            {
                Rect rightEye = GetEyeBox(image, result.faceLandmark[42], result.faceLandmark[45]);
                Point2f rightIris = irisDetector.IrisDetect(image.SubMat(rightEye));
                rightIris += rightEye.TopLeft;
                result.rightEyeDirection = irisDetector.IrisToDirection(result.faceLandmark[42], result.faceLandmark[45], rightIris, faceAngle);
                result.rightEyeIris = rightIris;
            });
            Task.WaitAll(leftEyeTask, rightEyeTask);

            return result;
        }

        private Rect SafeRect(Rect rect, int img_width, int img_height)
        {
            if (rect.X < 0) rect.X = 0;
            if (rect.Y < 0) rect.Y = 0;
            if (rect.X + rect.Width > img_width) rect.Width = img_width - rect.X;
            if (rect.Y + rect.Height > img_height) rect.Height = img_height - rect.Y;
            return rect;
        }

        private Rect BoxExtension(Rect box, int img_width, int img_height)
        {
            int width = box.Width, height = box.Height;
            int cx = box.X + width / 2;
            int cy = box.Y + height / 2;
            int size = width > height ? width : height;
            width = height = (int)(size * 1.1);
            return SafeRect(new Rect((int)(cx - width / 2), (int)(cy - width / 2), (int)width, (int)height), img_width, img_height);
        }

        private Rect GetEyeBox(Mat image, Point2f l, Point2f r)
        {
            int img_width = image.Width, img_height = image.Height;
            Point2f c = (l + r) * 0.5;
            double width = Math.Abs(l.X - r.X) * 64 / 23, height = width;
            return SafeRect(new Rect((int)(c.X - width / 2), (int)(c.Y - height / 2), (int)width, (int)height), img_width, img_height);
        }
    }
}
