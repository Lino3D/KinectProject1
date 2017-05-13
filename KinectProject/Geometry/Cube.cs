using System;
using OpenTK;

namespace KinectProject.Geometry
{
    public class Cube : ICloneable
    {
        public Vector3 Center { get; set; }
        //public Point[] Corners { get; set; }

        public CubePoint[, ,] Vertices { get; set; }

        public void Rotate(float angleX, float angleY, float angleZ)
        {

            var rotationX = Matrix3.CreateFromAxisAngle(new Vector3(1, 0, 0), angleX);
            var rotationY = Matrix3.CreateFromAxisAngle(new Vector3(0, 1, 0), angleY);
            var rotationZ = Matrix3.CreateFromAxisAngle(new Vector3(0, 0, 1), angleZ);

            foreach (var cubePoint in Vertices)
            {
                var result = new Vector3((float)cubePoint.X, (float)cubePoint.Y, (float)cubePoint.Z);
                result -= Center;
                if (angleX != 0) Vector3.Transform(ref result, ref rotationX, out result);
                if (angleY != 0) Vector3.Transform(ref result, ref rotationY, out result);
                if (angleZ != 0)Vector3.Transform(ref result, ref rotationZ, out result);
                result += Center;
                cubePoint.X = result.X;
                cubePoint.Y = result.Y;
                cubePoint.Z = result.Z;
            }
        }

        public object Clone()
        {
            //            var newCube = new Cube()
            //            {
            //                Center = Center,
            //                Vertices = (CubePoint[,,]) Vertices.Clone()
            //            };
            //            for (var x = 0; x < Vertices.GetLength(0); x++)
            //            {
            //                for (var y = 0; y < Vertices.GetLength(1); y++)
            //                {
            //                    for (var z = 0; z < Vertices.GetLength(2); z++)
            //                    {
            //                        newCube.Vertices[x, y, z] = (CubePoint) Vertices[x, y, z].Clone();
            //                    }
            //                }
            //            }
            //            return newCube;
            return this.MemberwiseClone();
        }
    }
}