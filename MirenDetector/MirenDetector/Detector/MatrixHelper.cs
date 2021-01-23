using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace MirenDetector
{
    enum ImageFormat
    {
        BGR,
        RGB
    }

    class MatrixHelper
    {
        public static Mat Resize(Mat image, int width, int height)
        {
            if (image.Width == width && image.Height == height) return image;
            return image.Resize(new Size(width, height));
        }

        public static Tensor<float> MatToTensor(ImageFormat format, float mean, float std, params Mat[] image)
        {
            if (image[0].Type() == MatType.CV_8UC3)
            {
                int[] shape = new int[] { image.Length, 3, image[0].Height, image[0].Width };
                int size = image[0].Height * image[0].Width;
                float[] data = new float[image.Length * 3 * size];
                for(int t = 0; t < image.Length; t++)
                {
                    Vec3b[] arr; image[t].GetArray(out arr);
                    for (int i = 0; i < size; i++)
                    {
                        if (format == ImageFormat.BGR)
                        {
                            data[i + size * (t * 3 + 0)] = (arr[i].Item0 / 255.0f - mean) / std;
                            data[i + size * (t * 3 + 1)] = (arr[i].Item1 / 255.0f - mean) / std;
                            data[i + size * (t * 3 + 2)] = (arr[i].Item2 / 255.0f - mean) / std;
                        }
                        else if (format == ImageFormat.RGB)
                        {
                            data[i + size * (t * 3 + 0)] = (arr[i].Item2 / 255.0f - mean) / std;
                            data[i + size * (t * 3 + 1)] = (arr[i].Item1 / 255.0f - mean) / std;
                            data[i + size * (t * 3 + 2)] = (arr[i].Item0 / 255.0f - mean) / std;
                        }
                    }
                }
                return new DenseTensor<float>(data, shape);
            }

            else if (image[0].Type() == MatType.CV_32FC3)
            {
                int[] shape = new int[] { image.Length, 3, image[0].Height, image[0].Width };
                int size = image[0].Height * image[0].Width;
                float[] data = new float[image.Length * 3 * size];
                for (int t = 0; t < image.Length; t++)
                {
                    Vec3f[] arr; image[t].GetArray(out arr);
                    for (int i = 0; i < size; i++)
                    {
                        if (format == ImageFormat.BGR)
                        {
                            data[i + size * (t * 3 + 0)] = (arr[i].Item0 - mean) / std;
                            data[i + size * (t * 3 + 1)] = (arr[i].Item1 - mean) / std;
                            data[i + size * (t * 3 + 2)] = (arr[i].Item2 - mean) / std;
                        }
                        else if (format == ImageFormat.RGB)
                        {
                            data[i + size * (t * 3 + 0)] = (arr[i].Item2 - mean) / std;
                            data[i + size * (t * 3 + 1)] = (arr[i].Item1 - mean) / std;
                            data[i + size * (t * 3 + 2)] = (arr[i].Item0 - mean) / std;
                        }
                    }
                }
                return new DenseTensor<float>(data, shape);
            }
            else throw new ArgumentException("Mat type not supported");
        }

        public static Tensor<float> GrayImageToTensor(Mat image, bool squeeze = true, float mean = 0.0f, float std = 1.0f)
        {
            int[] shape;
            if (squeeze) shape = new int[] { 1, image.Height, image.Width };
            else shape = new int[] { 1, 1, image.Height, image.Width };
            int size = image.Height * image.Width;
            float[] data = new float[size];
            byte[] arr; image.GetArray(out arr);
            for (int i = 0; i < size; i++) data[i] = (arr[i] / 255.0f - mean) / std;
            return new DenseTensor<float>(data, shape);
        }
    }
}
