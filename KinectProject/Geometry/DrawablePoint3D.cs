﻿using System;
using OpenTK;

namespace KinectProject.Geometry
{
    public class DrawablePoint3D : ICloneable
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public bool DrawFlag { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}