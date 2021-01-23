using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace MirenDetector
{
    class CameraMatrix
    {
        public double[,] arr;
        public CameraMatrix(double[,] _arr) => arr = (double[,]) _arr.Clone();
        public static CameraMatrix DefaultCamera(int width, int height)
        {
            return new CameraMatrix(new double[3, 3]
            {
                {width, 0, width / 2.0 },
                {0, width, height / 2.0 },
                {0, 0, 1 }
            });
        }

        public Mat GetMatrix()
        {
            return Mat.FromArray(arr);
        }

        private void Calibration()
        {
            // TODO
        }
        /*
        public List<string> CameraFileList()
        {

        }

        public void Save(string name)
        {

        }

        public void Load(string name)
        {

        }*/
    }
}
