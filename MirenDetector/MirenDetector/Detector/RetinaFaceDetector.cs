using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace MirenDetector
{

    public class RetinaFaceDetector
    {
        InferenceSession session;
        
        public RetinaFaceDetector()
        {
            SessionOptions so = new SessionOptions();
            so.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED;
            so.AppendExecutionProvider_CUDA(0);
            session = new InferenceSession("Assets/RFB640.onnx", so);
        }


        public Rect? FaceDetect(Mat image)
        {
            int image_width = image.Width;
            int image_height = image.Height;
            image = MatrixHelper.Resize(image, 640, 480);

            Tensor<float> input = MatrixHelper.MatToTensor(ImageFormat.RGB, 127.0f / 255.0f, 0.5f, image);
            var inputs = new List<NamedOnnxValue>() { NamedOnnxValue.CreateFromTensor<float>("input", input) };

            float[] scores = null, boxes = null;

            using (var results = session.Run(inputs))
            {
                foreach (var r in results)
                {
                    float[] value = r.AsTensor<float>().ToDenseTensor().Buffer.ToArray();
                    if (r.Name == "scores") scores = value;
                    if (r.Name == "boxes") boxes = value;
                }

                int size = 0;
                Rect? bounding_box = null;

                for (int i = 0; i < 17640; i++)
                {
                    if (scores[i * 2 + 1] < 0.5) continue;
                    int x = (int)(image_width * boxes[i * 4]);
                    int y = (int)(image_height * boxes[i * 4 + 1]);
                    int z = (int)(image_width * boxes[i * 4 + 2]);
                    int w = (int)(image_height * boxes[i * 4 + 3]);

                    if (x < 0) x = 0;
                    if (y < 0) y = 0;
                    if (z < 0) z = 0;
                    if (w < 0) w = 0;

                    if (x > z) (x, z) = (z, x);
                    if (y > w) (y, w) = (w, y);

                    int currentSize = (z - x) * (w - y);
                    if (size > currentSize) continue;
                    size = currentSize;
                    bounding_box = new Rect(x, y, z - x + 1, w - y + 1);
                }
                return bounding_box;
            }
        }
    }
}
