﻿using System;
using OpenTK;

namespace KinectProject.Geometry
{
    public class Rectangle : ICloneable
    {
        public Vector3 Center { get; set; }
        //public Point[] Corners { get; set; }

        public DrawablePoint3D[, ,] Vertices { get; set; }

        public void Rotate(float angleX, float angleY, float angleZ)
        {
            var rotationX = Matrix3.CreateFromAxisAngle(new Vector3(1, 0, 0), angleX);
            var rotationY = Matrix3.CreateFromAxisAngle(new Vector3(0, 1, 0), angleY);
            var rotationZ = Matrix3.CreateFromAxisAngle(new Vector3(0, 0, 1), angleZ);

            foreach (var point in Vertices)
            {
                var result = new Vector3((float)point.X, (float)point.Y, (float)point.Z);
                result -= Center;
                Vector3.Transform(ref result, ref rotationX, out result);
                Vector3.Transform(ref result, ref rotationY, out result);
                Vector3.Transform(ref result, ref rotationZ, out result);
                result += Center;
                point.X = result.X;
                point.Y = result.Y;
                point.Z = result.Z;
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}