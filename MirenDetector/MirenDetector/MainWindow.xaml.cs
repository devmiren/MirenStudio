using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;
using OpenCvSharp;

namespace MirenDetector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private const int IMAGE_WIDTH = 640;
        private const int IMAGE_HEIGHT = 480;
        private const int CAMERA_FPS = 30;

        MirenDetectior mirenDetector;

        VideoCapture webcam;
        int currentWebcamIdx;
        CancellationTokenSource CaptureMainLoopToken;
        Task sendResultTask;

        Mat currentFrame;
        NamedPipeServerStream pipeServer;
        DetectionResult currentResult;

        private Ellipse[] landmarks;
        private Line[] faceCoordinate, eyeDirection;

        private WriteableBitmap ColorMatToWriteableBitmap(Mat frame)
        {
            Vec3b[] arr;
            frame.GetArray<Vec3b>(out arr);

            WriteableBitmap bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr24, null);
            bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), arr, 3 * frame.Width, 0);
            return bitmap;
        }
        private WriteableBitmap GrayMatToWriteableBitmap(Mat frame)
        {
            byte[] arr;
            frame.GetArray<byte>(out arr);

            WriteableBitmap bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Gray8, null);
            bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), arr, frame.Width, 0);
            return bitmap;
        }

        public MainWindow()
        {
            this.InitializeComponent();
            pipeServer = new NamedPipeServerStream("MirenPipe", PipeDirection.InOut, 1);

            InitializeResultComponent();
            mirenDetector = new MirenDetectior(IMAGE_WIDTH, IMAGE_HEIGHT);
            SetWebcamList();
        }

        private void InitializeResultComponent()
        {
            landmarks = new Ellipse[68];
            for (int i = 0; i < 68; i++)
            {
                landmarks[i] = new Ellipse();
                landmarks[i].Fill = new SolidColorBrush(Colors.Red);
                landmarks[i].Width = 2;
                landmarks[i].Height = 2;
                resultCanvas.Children.Add(landmarks[i]);
            }

            faceCoordinate = new Line[3];
            for (int i = 2; i >= 0; i--)
            {
                faceCoordinate[i] = new Line();
                faceCoordinate[i].StrokeThickness = 3.0;
                resultCanvas.Children.Add(faceCoordinate[i]);
            }
            faceCoordinate[0].Stroke = new SolidColorBrush(Colors.Red);
            faceCoordinate[1].Stroke = new SolidColorBrush(Colors.Green);
            faceCoordinate[2].Stroke = new SolidColorBrush(Colors.Blue);

            eyeDirection = new Line[2];
            for (int i = 1; i >= 0; i--)
            {
                eyeDirection[i] = new Line();
                eyeDirection[i].StrokeThickness = 1.0;
                eyeDirection[i].Stroke = new SolidColorBrush(Colors.Red);
                resultCanvas.Children.Add(eyeDirection[i]);
            }
        }

        private void SetWebcamList()
        {
            for(int idx = 0; idx < 10; idx++)
            {
                VideoCapture webcam = new VideoCapture(idx);
                if (!webcam.IsOpened()) continue;
                webcamList.Items.Add("webcam " + idx.ToString());
                webcam.Dispose();
            }
        }

        private bool SetWebcam(int idx)
        {
            if (currentWebcamIdx == idx && webcam != null && webcam.IsOpened()) return true;
            VideoCapture nxtWebcam = new VideoCapture(idx);
            if (!nxtWebcam.IsOpened()) return false;

            if (webcam != null && webcam.IsOpened()) CaptureMainLoopToken.Cancel();

            currentWebcamIdx = idx;
            webcam = nxtWebcam;
            webcam.Set(VideoCaptureProperties.FrameWidth, IMAGE_WIDTH);
            webcam.Set(VideoCaptureProperties.FrameHeight, IMAGE_HEIGHT);
            webcam.Set(VideoCaptureProperties.Fps, CAMERA_FPS);

            CaptureMainLoopToken = new CancellationTokenSource();
            Task.Run(() => CaptureMainLoop(webcam, CaptureMainLoopToken.Token));
            return true;
        }

        private void webcamListSelection(object sender, SelectionChangedEventArgs e)
        {
            int idx = int.Parse(webcamList.SelectedItem.ToString().Split(' ')[1]);
            if (!SetWebcam(idx)) SetWebcamList();
        }

        private async void CaptureMainLoop(VideoCapture currentWebcam, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Mat mat = new Mat();
                if(!currentWebcam.Read(mat)) continue;
                currentFrame = mat.Flip(FlipMode.Y);
                currentResult = await mirenDetector.DetectAsync(currentFrame);

                DisplayFrame();
                DisplayDetectionResult();

                if (currentResult == null) continue;
                if (sendResultTask == null || sendResultTask.IsCompleted) sendResultTask = SendToclient();
            }
            currentWebcam.Dispose();
        }

        private void DisplayFrame()
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (showCameraImage.IsChecked.Value) previewImage.Source = ColorMatToWriteableBitmap(currentFrame);
                else previewImage.Source = null;
            });
        }

        private void DrawLine(Line line, Point2f start, Point2f end)
        {
            line.X1 = start.X; line.Y1 = start.Y;
            line.X2 = end.X; line.Y2 = end.Y;
        }

        private void DisplayDetectionResult()
        {
            if(currentResult == null || !currentResult.face.HasValue)
            {
                Dispatcher.BeginInvoke(() => resultCanvas.Visibility = Visibility.Hidden);
                return;
            }

            Dispatcher.BeginInvoke(() =>
            {
                if (!showDetectionResult.IsChecked.Value)
                {
                    resultCanvas.Visibility = Visibility.Hidden;
                    return;
                }
                resultCanvas.Visibility = Visibility.Visible;
                OpenCvSharp.Rect rect = currentResult.face.Value;
                faceRect.Width = rect.Width * 640 / IMAGE_WIDTH;
                faceRect.Height = rect.Height * 480 / IMAGE_HEIGHT;
                Canvas.SetLeft(faceRect, rect.X * 640 / IMAGE_WIDTH);
                Canvas.SetTop(faceRect, rect.Y * 480 / IMAGE_HEIGHT);

                FacialLandmark landmark = currentResult.faceLandmark;
                for (int i = 0; i < 68; i++)
                {
                    Canvas.SetLeft(landmarks[i], landmark[i].X * 640 / IMAGE_WIDTH - 1);
                    Canvas.SetTop(landmarks[i], landmark[i].Y * 480 / IMAGE_HEIGHT - 1);
                }

                Quaternion q = currentResult.faceAngle;
                Point2f O = new Point2f(landmark[30].X * 640 / IMAGE_WIDTH, landmark[30].Y * 480 / IMAGE_HEIGHT);
                for (int i = 0; i < 3; i++)
                {
                    float x = 0, y = 0, z = 0;
                    if (i == 0) x = 100;
                    else if (i == 1) y = 100;
                    else if (i == 2) z = 100;
                    Quaternion resultQ = q * new Quaternion(x, y, z, 0.0f) / q;
                    Point2f diff = new Point2f(resultQ.X, resultQ.Y);
                    DrawLine(faceCoordinate[i], O, O + diff);
                }

                Point2f leftIris = currentResult.leftEyeIris, rightIris = currentResult.rightEyeIris;
                Point2f leftDirection = currentResult.leftEyeDirection, rightDirection = currentResult.rightEyeDirection;
                Point2f leftEnd = new Point2f(leftIris.X + (float)Math.Sin(leftDirection.X) * 100, leftIris.Y - (float)Math.Sin(leftDirection.Y) * 100);
                Point2f rightEnd = new Point2f(rightIris.X + (float)Math.Sin(rightDirection.X) * 100, rightIris.Y - (float)Math.Sin(rightDirection.Y) * 100);

                DrawLine(eyeDirection[0], leftIris, leftEnd);
                DrawLine(eyeDirection[1], rightIris, rightEnd);
            });
        }

        private async Task SendToclient()
        {
            CancellationToken token = CaptureMainLoopToken.Token;
            try
            {
                if (!pipeServer.IsConnected) await pipeServer.WaitForConnectionAsync(token);
                byte[] res = currentResult.ToByteArray();
                await pipeServer.WriteAsync(res, 0, res.Length);
            }

            catch (Exception e)
            {
                pipeServer.Close();
                pipeServer.Dispose();

                pipeServer = new NamedPipeServerStream("MirenPipe", PipeDirection.InOut, 1);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if(webcam != null) CaptureMainLoopToken.Cancel();
        }
    }
}
