using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectProject.Helpers;

namespace KinectProject.Geometry
{
    public class ObjectInfo
    {
        public Rectangle ObjectGeometry { get; set; }
        public double[,] DepthMap { get; set; }
    }
}