using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectProject.Helpers;

namespace KinectProject.Geometry
{
    public class DepthData
    {
        public Rectangle Rect { get; set; }
        public double[,] DepthMap { get; set; }

        public static Rectangle ProcessData(List<DepthData> depthDataList)
        {
            var first = depthDataList.FirstOrDefault();
            if (first == null) return null;
            var firstRectangle = first.Rect;
            var lengthX = firstRectangle.Vertices.GetLength(0);
            var lengthY = firstRectangle.Vertices.GetLength(1);
            var lengthZ = firstRectangle.Vertices.GetLength(2);

            var result = new Rectangle
            {
                Center = firstRectangle.Center,
                Vertices = new DrawablePoint3D[lengthX, lengthY, lengthZ]
            };

            for (var x = 0; x < lengthX; x++)
            {
                for (var y = 0; y < lengthY; y++)
                {
                    for (var z = 0; z < lengthZ; z++)
                    {
                        result.Vertices[x, y, z] = (DrawablePoint3D)firstRectangle.Vertices[x, y, z].Clone();
                        result.Vertices[x, y, z].DrawPoint = true;
                    }
                }
            }

            foreach (var depthData in depthDataList)
            {
                for (var x = 0; x < lengthX; x++)
                {
                    for (var y = 0; y < lengthY; y++)
                    {
                        for (var z = 0; z < lengthZ; z++)
                        {
                            var rectVertex = depthData.Rect.Vertices[x, y, z];
                            if (!rectVertex.InCube() ||
                                rectVertex.Z < depthData.DepthMap[(int)rectVertex.X, (int)rectVertex.Y])
                            {
                                result.Vertices[x, y, z].DrawPoint = false;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}