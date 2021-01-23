using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace MirenDetector
{
    class FacialLandmarkDetector
    {
        InferenceSession session;

        public FacialLandmarkDetector()
        {
            SessionOptions so = new SessionOptions();
            so.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED;
            so.AppendExecutionProvider_CUDA(0);
            session = new InferenceSession("Assets/pfld.onnx", so);
        }

        public FacialLandmark LandmarkDetect(Mat image)
        {
            int img_width = image.Width, img_height = image.Height;
            image = MatrixHelper.Resize(image, 112, 112);

            Tensor<float> input = MatrixHelper.MatToTensor(ImageFormat.BGR, 0, 1, image);
            var inputs = new List<NamedOnnxValue>() { NamedOnnxValue.CreateFromTensor<float>("input", input) };
            FacialLandmark landmark = new FacialLandmark();

            using (var results = session.Run(inputs))
            {
                float[] output = null;
                foreach (var r in results)
                {
                    float[] value = r.AsTensor<float>().ToDenseTensor().Buffer.ToArray();
                    if (r.Name == "output") output = value;
                }
                for (int i = 0; i < 68; i++)
                {
                    landmark.location[i].X = output[i * 2] * img_width;
                    landmark.location[i].Y = output[i * 2 + 1] * img_height;
                }
            }
            return landmark;
        }
    }
}
