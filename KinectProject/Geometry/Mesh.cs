using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectProject.Geometry
{
    public class Mesh
    {
        public Vector3[] Vertices { get; set; }
        public int[] Triangles { get; set; }
    }
}