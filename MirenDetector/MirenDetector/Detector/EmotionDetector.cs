using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace MirenDetector
{

    public class EmotionDetector
    {
        InferenceSession session;

        public EmotionDetector()
        {
            SessionOptions so = new SessionOptions();
            so.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED; 
            so.AppendExecutionProvider_CUDA(0);
            session = new InferenceSession("Assets/emotionferplus8.onnx", so);
        }

        public EmotionVector EmotionDetect(Mat image)
        {
            image = MatrixHelper.Resize(image.CvtColor(ColorConversionCodes.BGR2GRAY), 64, 64);

            Tensor<float> input = MatrixHelper.GrayImageToTensor(image, false, 0, 1 / 255.0f);
            var inputs = new List<NamedOnnxValue>() { NamedOnnxValue.CreateFromTensor<float>("Input3", input) };

            EmotionVector emotion = new EmotionVector();

            using (var results = session.Run(inputs))
            {
                float[] arr = null;
                foreach (var r in results)
                {
                    float[] value = r.AsTensor<float>().ToDenseTensor().Buffer.ToArray();
                    if (r.Name == "Plus692_Output_0") arr = value;
                }
                double total = 0;
                for (int i = 0; i < 8; i++) total += Math.Exp(arr[i]);
                for (int i = 0; i < 8; i++) emotion[i] = (float)(Math.Exp(arr[i]) / total);
                return emotion;
            }
        }
    }
}
