using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace MirenDetector
{
    class Face3DPoint
    {
        public static int[] keypointIdx = { 17, 21, 22, 26, 36, 39, 42, 45, 31, 35, 48, 54, 57, 8 };
        public Point3f[] faceCoordinate = new Point3f[14]
        {
            new Point3f(6.825897f, 6.760612f, 4.402142f),     //#17 left brow left corner
            new Point3f(1.330353f, 7.122144f, 6.903745f),     //#21 left brow right corner
            new Point3f(-1.330353f, 7.122144f, 6.903745f),    //#22 right brow left corner
            new Point3f(-6.825897f, 6.760612f, 4.402142f),    //#26 right brow right corner
            new Point3f(5.311432f, 5.485328f, 3.987654f),     //#36 left eye left corner
            new Point3f(1.789930f, 5.393625f, 4.413414f),     //#39 left eye right corner
            new Point3f(-1.789930f, 5.393625f, 4.413414f),    //#42 right eye left corner
            new Point3f(-5.311432f, 5.485328f, 3.987654f),    //#45 right eye right corner
            new Point3f(2.005628f, 1.409845f, 6.165652f),     //#31 nose left corner
            new Point3f(-2.005628f, 1.409845f, 6.165652f),    //#35 nose right corner
            new Point3f(2.774015f, -2.080775f, 5.048531f),    //#48 mouth left corner
            new Point3f(-2.774015f, -2.080775f, 5.048531f),   //#54 mouth right corner
            new Point3f(0.000000f, -3.116408f, 6.097667f),    //#57 mouth central bottom corner
            new Point3f(0.000000f, -7.415691f, 4.070434f)     //#8 chin corner
        };
        public Face3DPoint(Point3f[] _faceCoordinate) => faceCoordinate = (Point3f[])_faceCoordinate.Clone();
        public Face3DPoint() { }
        public Point3f this[int idx]
        {
            get => faceCoordinate[idx];
        }

        /*
        public List<string> faceFileList()
        {

        }

        public void Save(string name)
        {

        }

        public static void Load(string name)
        {

        }*/
    }
}
